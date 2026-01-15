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
    
    // Fix: Initialize as null! to silence CS8618 warnings (they are set in InitializeRabbitMq)
    private IConnection _connection = null!;
    private IModel _channel = null!;

    public RabbitMqService(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqService> logger)
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
                // FIX: Use correct property names from RabbitMqSettings class
                HostName = _settings.Host,       
                Port = _settings.Port,           
                UserName = _settings.Username,   
                Password = _settings.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare Exchange
            _channel.ExchangeDeclare(exchange: "optimization.exchange", type: ExchangeType.Topic, durable: true);

            _logger.LogInformation("Connected to RabbitMQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not connect to RabbitMQ");
            throw; 
        }
    }

    public void Publish<T>(string exchange, string routingKey, T message) where T : IMessage
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(exchange: exchange,
                              routingKey: routingKey,
                              basicProperties: null,
                              body: body);

        _logger.LogInformation($"Published message to {exchange}/{routingKey}");
    }

    public Task PublishAsync<T>(string routingKey, T message) where T : IMessage
    {
        Publish("optimization.exchange", routingKey, message);
        return Task.CompletedTask;
    }

    public void Subscribe<T>(string queueName, Action<T> handler) where T : IMessage
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            try
            {
                var message = JsonSerializer.Deserialize<T>(json);
                if (message != null)
                {
                    handler(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
            }
        };

        // Ensure queue infrastructure exists
        DeclareQueue(queueName);
        BindQueue(queueName, Exchanges.Optimization, queueName);

        _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        
        _logger.LogInformation($"Subscribed and Bound queue: {queueName}");
    }

    // --- FIX: Missing Interface Implementations ---

    public void DeclareExchange(string exchangeName, string type)
    {
        _channel.ExchangeDeclare(exchange: exchangeName, type: type, durable: true);
    }

    public void DeclareQueue(string queueName)
    {
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    public void BindQueue(string queueName, string exchangeName, string routingKey)
    {
        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}