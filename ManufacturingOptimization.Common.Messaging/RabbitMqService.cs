using System.Text;
using System.Text.Json;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ManufacturingOptimization.Common.Messaging;

public class RabbitMqService : IMessagePublisher, IMessageSubscriber, IMessagingInfrastructure, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqService> _logger;
    private readonly Dictionary<string, EventingBasicConsumer> _consumers = new();

    public RabbitMqService(IOptions<RabbitMqSettings> config, ILogger<RabbitMqService> logger)
    {
        _logger = logger;
        var settings = config.Value;
        
        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            UserName = settings.Username,
            Password = settings.Password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void DeclareExchange(string exchangeName, string exchangeType = "topic")
    {
        _channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType, durable: true);
    }

    public void DeclareQueue(string queueName)
    {
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
    }

    public void BindQueue(string queueName, string exchangeName, string routingKey)
    {
        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);
    }

    public void PurgeQueue(string queueName)
    {
        _channel.QueuePurge(queueName);
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
        // Reuse existing consumer if queue already has one, otherwise create new
        if (!_consumers.TryGetValue(queueName, out var consumer))
        {
            consumer = new EventingBasicConsumer(_channel);
            _consumers[queueName] = consumer;

            consumer.Received += (model, e) =>
            {
                try
                {
                    var body = e.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    
                    // Try to get message type from header
                    string? messageType = null;
                    if (e.BasicProperties?.Headers?.TryGetValue("MessageType", out var typeObj) == true)
                    {
                        messageType = Encoding.UTF8.GetString((byte[])typeObj);
                    }

                    // Try to deserialize and invoke matching handler
                    var typeName = typeof(T).FullName ?? typeof(T).Name;
                    if (messageType == null || messageType == typeName)
                    {
                        var message = JsonSerializer.Deserialize<T>(json);
                        if (message != null)
                        {
                            handler(message);
                        }
                    }

                    _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue {Queue}", queueName);
                    _channel.BasicNack(deliveryTag: e.DeliveryTag, multiple: false, requeue: false);
                }
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _logger.LogInformation("Disconnected from RabbitMQ");
    }
}
