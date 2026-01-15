using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;

namespace ManufacturingOptimization.Common.Messaging;

public class RabbitMqService : IMessagePublisher, IMessageSubscriber, IMessagingInfrastructure, IDisposable
{
    private readonly ILogger<RabbitMqService> _logger;
    private readonly RabbitMqSettings _settings;

    private readonly Dictionary<string, EventingBasicConsumer> _consumers = new();

    private IConnection _connection = null!;
    private IModel _channel = null!;

    public RabbitMqService(
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        InitializeRabbitMq();
    }

    private void InitializeRabbitMq()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: Exchanges.Optimization,
                type: ExchangeType.Topic,
                durable: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not connect to RabbitMQ");
            throw;
        }
    }

    public void Publish<T>(string exchangeName, string routingKey, T message) where T : IMessage
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Headers = new Dictionary<string, object>
        {
            ["MessageType"] = typeof(T).FullName ?? typeof(T).Name
        };

        _channel.BasicPublish(
            exchange: exchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);
    }

    public void Subscribe<T>(string queueName, Action<T> handler) where T : IMessage
    {
        DeclareQueue(queueName);
        BindQueue(queueName, Exchanges.Optimization, queueName);

        if (_consumers.ContainsKey(queueName))
            return;

        var consumer = new EventingBasicConsumer(_channel);
        _consumers[queueName] = consumer;

        consumer.Received += (sender, e) =>
        {
            try
            {
                var body = e.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                string? messageType = null;
                if (e.BasicProperties?.Headers?.TryGetValue("MessageType", out var typeObj) == true)
                {
                    messageType = Encoding.UTF8.GetString((byte[])typeObj);
                }

                var expectedType = typeof(T).FullName ?? typeof(T).Name;

                if (messageType == null || messageType == expectedType)
                {
                    var message = JsonSerializer.Deserialize<T>(json);
                    if (message != null)
                    {
                        handler(message);
                    }
                }

                _channel.BasicAck(e.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing message from queue {Queue}",
                    queueName);

                _channel.BasicNack(
                    deliveryTag: e.DeliveryTag,
                    multiple: false,
                    requeue: false);
            }
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);
    }

    public void DeclareExchange(string exchangeName, string type)
    {
        _channel.ExchangeDeclare(exchangeName, type, durable: true);
    }

    public void DeclareQueue(string queueName)
    {
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public void BindQueue(
        string queueName,
        string exchangeName,
        string routingKey)
    {
        _channel.QueueBind(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey);
    }

    public void PurgeQueue(string queueName)
    {
        _channel.QueuePurge(queueName);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
