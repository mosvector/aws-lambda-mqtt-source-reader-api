using MQTTnet;
using MQTTnet.Protocol;

namespace MQTTSourceAPI.Repository
{
    public sealed class MessageQueueRepository : IMessageQueueRepository
    {
        // The MQTT client instance
        private static IMessageQueueClient _client;

        public MessageQueueRepository(IMessageQueueClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _client = client;
        }

        public async Task WriteToQueueAsync(string input)
        {
            await _client.ConnectAsync();

            var mqttTopic = Environment.GetEnvironmentVariable("MQTT_TOPIC") ?? throw new InvalidOperationException("MQTT_TOPIC environment variable is not set.");
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(mqttTopic)
                .WithPayload(input)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build();
             
            await _client.PublishAsync(message);

            await _client.DisconnectAsync();
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
