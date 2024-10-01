namespace MQTTSourceAPI.Repository
{
    public interface IMessageQueueRepository : IDisposable
    {
        Task WriteToQueueAsync(string input);
    }
}
