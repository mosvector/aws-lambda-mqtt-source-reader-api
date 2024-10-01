using MQTTnet;

namespace MQTTSourceAPI.Repository
{
    public interface IMessageQueueClient : IDisposable
    {
        Task ConnectAsync();
        Task DisconnectAsync();
        Task PublishAsync(MqttApplicationMessage message);
    }
}
