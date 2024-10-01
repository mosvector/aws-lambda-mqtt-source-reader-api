using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MQTTSourceAPI.Repository;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MQTTSourceAPI;

/// <summary>
/// A simple function that takes a string, publishes it to an MQTT server, and returns "ok".
/// </summary>
public class Function
{
    /// <summary>
    /// Message queue repository
    /// </summary>
    private readonly IMessageQueueRepository _repo;

    /// <summary>
    /// Constructs a new instance of <see cref="Function" /> with a default <see cref="IMessageQueueRepository" />.
    /// </summary>
    public Function()
    {
        // Create a new instance of the message queue repository
        _repo = new MessageQueueRepository(new MessageQueueClient());
    }

    /// <summary>
    /// A simple function that takes a string, publishes it to an MQTT server, and returns "ok".
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            // Get the input from the request
            var input = request.Body ?? throw new ArgumentNullException("request.Body", nameof(request.Body));

            // Publish input to the queue
            LambdaLogger.Log($"Write: {input} to mqtt queue");

            // Write the input to the queue
            await _repo.WriteToQueueAsync(input);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "ok",
            };
             
        }
        catch (Exception ex)
        {
            LambdaLogger.Log($"Error: {ex.Message}");

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = "error",
            };
        }
    }
}
