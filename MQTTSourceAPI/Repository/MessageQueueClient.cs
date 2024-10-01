using MQTTnet.Client;
using MQTTnet;
using Amazon.Lambda.Core;

namespace MQTTSourceAPI.Repository
{
    public class MessageQueueClient : IMessageQueueClient
    {
        private IMqttClient _mqttClient;

        public MessageQueueClient()
        {
            _mqttClient = new MqttFactory().CreateMqttClient();
        }

        public async Task ConnectAsync()
        {
            var brokerAddress = Environment.GetEnvironmentVariable("MQTT_BROKER_ADDRESS");
            var brokerPortString = Environment.GetEnvironmentVariable("MQTT_BROKER_PORT");

            if (brokerAddress == null || brokerPortString == null)
            {
                throw new InvalidOperationException("MQTT_BROKER_ADDRESS or MQTT_BROKER_PORT environment variable is not set.");
            }

            var mqttClientId = $"MQTTG0-{Guid.NewGuid()}";
            var brokerPort = int.Parse(brokerPortString);
            var mqttServerOptions = new MqttClientOptionsBuilder()
               .WithTcpServer(brokerAddress, brokerPort)
               .WithClientId(mqttClientId)
               .Build();
            await _mqttClient.ConnectAsync(mqttServerOptions);
            LambdaLogger.Log("MQTT server connected.");
        }

        public async Task PublishAsync(MqttApplicationMessage message)
        {
            if (!_mqttClient.IsConnected)
            {
                throw new InvalidOperationException("MQTT client is not connected.");
            }

            await _mqttClient.PublishAsync(message);
        }

        public async Task DisconnectAsync()
        {           
            await _mqttClient.DisconnectAsync();
            LambdaLogger.Log("MQTT server disconnected.");
        }

        public void Dispose()
        {
            _mqttClient.Dispose();
        }
    }
}
