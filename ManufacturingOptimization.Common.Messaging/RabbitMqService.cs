using System.Text;
using System.Text.Json;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ManufacturingOptimization.Common.Messaging;

// Ensure this class implements IMessagePublisher
//public class RabbitMqService : IMessagePublisher, IMessageSubscriber, IMessagingInfrastructure, IDisposable
// FIX: Implement BOTH interfaces to satisfy Gateway (New) and Engine (Old)
public class RabbitMqService : 
    global::Common.Messaging.IMessagePublisher, 
    ManufacturingOptimization.Common.Messaging.Abstractions.IMessagePublisher,
    IMessageSubscriber, 
    IMessagingInfrastructure, 
    IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqService> _logger;
    
    // Default exchange for events if none is specified
    private const string DefaultExchange = "manufacturing.events";

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

    // --- NEW METHOD: Implements IMessagePublisher ---
    public Task PublishAsync<T>(T message)
    {
        // 1. Ensure the default exchange exists (idempotent)
        DeclareExchange(DefaultExchange);

        // 2. Use the class name as the routing key (e.g., "CustomerRequestSubmittedEvent")
        string routingKey = typeof(T).Name;

        // 3. Serialize
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        // 4. Publish (RabbitMQ Client < 7.0 is synchronous, so we return CompletedTask)
        _channel.BasicPublish(
            exchange: DefaultExchange,
            routingKey: routingKey,
            basicProperties: null,
            body: body);

        _logger.LogInformation("Published {MessageType} to {Exchange}", routingKey, DefaultExchange);

        return Task.CompletedTask;
    }

    // Existing manual Publish method (kept for backward compatibility)
    public void Publish<T>(string exchangeName, string routingKey, T message) where T : IMessage
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(
            exchange: exchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: body);
    }

    public void Subscribe<T>(string queueName, Action<T> handler) where T : IMessage
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (model, e) =>
        {
            try
            {
                var body = e.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<T>(json);

                if (message != null)
                {
                    handler(message);
                }

                _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message");
                _channel.BasicNack(deliveryTag: e.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _logger.LogInformation("Disconnected from RabbitMQ");
    }
}