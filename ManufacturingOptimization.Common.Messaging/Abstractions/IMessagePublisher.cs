namespace ManufacturingOptimization.Common.Messaging.Abstractions;

public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to the specified exchange with the given routing key.
    /// </summary>
    /// <remarks>This method sends the specified message to the given exchange using the provided routing key.
    /// Ensure that the exchange and routing key are correctly configured in the messaging system to  route the message
    /// to the intended destination.</remarks>
    /// <typeparam name="T">The type of the message to be published. Must implement the <see cref="IMessage"/> interface.</typeparam>
    /// <param name="exchangeName">The name of the exchange to which the message will be published. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="routingKey">The routing key used to route the message to the appropriate queue. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="message">The message to be published. Cannot be <see langword="null"/>.</param>
    void Publish<T>(string exchangeName, string routingKey, T message) where T : IMessage;
    
    /// <summary>
    /// Sends a request and waits for a response using RPC pattern.
    /// </summary>
    /// <typeparam name="TResponse">Expected response message type</typeparam>
    /// <param name="exchangeName">Exchange to publish to</param>
    /// <param name="routingKey">Routing key</param>
    /// <param name="request">Request message</param>
    /// <param name="timeout">Maximum time to wait for response</param>
    /// <returns>Response message or null if timeout</returns>
    Task<TResponse?> RequestReplyAsync<TResponse>(
        string exchangeName, 
        string routingKey, 
        IMessage request, 
        TimeSpan? timeout = null) where TResponse : class, IMessage;
    
    /// <summary>
    /// Publishes a reply message directly to a queue (used for RPC responses).
    /// </summary>
    void PublishReply<T>(string replyToQueue, string correlationId, T message) where T : IMessage;
}
