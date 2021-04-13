namespace RabbitMQ.EventBus.Extension.AspNetCore.Configurations
{
    /// <summary>
    /// 队列名前缀
    /// </summary>
    public enum QueuePrefixType
    {
        /// <summary>
        /// 交换机名
        /// </summary>
        ExchangeName,
        /// <summary>
        /// 客户端名称
        /// </summary>
        ClientProvidedName
    }
}
