using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AccessAuth;

public class Function
{
    /// <summary>
    /// Secret key used to validate the JWT token
    /// </summary>
    private readonly string SecretKey;

    /// <summary>
    /// Issuer of the JWT token
    /// </summary>
    private readonly string Issuer;

    public Function()
    {
        // Get the secret key and issuer from the environment variables
        SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? throw new ArgumentNullException("JWT_SECRET_KEY");
        Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new ArgumentNullException("JWT_ISSUER");
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public APIGatewayCustomAuthorizerV2IamResponse FunctionHandler(APIGatewayCustomAuthorizerV2Request request, ILambdaContext context)
    {
        // Get the JWT token from the headers
        if (!request.Headers.TryGetValue("authorization", out var token) || string.IsNullOrEmpty(token))
        {
            context.Logger.LogLine("Authorization header is missing or empty.");
            token = "";
        }

        // Remove "Bearer " prefix if present
        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token.Substring("Bearer ".Length).Trim();
        }

        // Create a token handler
        var tokenHandler = new JwtSecurityTokenHandler();
        ClaimsPrincipal? claimsPrincipal = null;
        try
        {
            // Set the validation parameters
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, // Validate the signing key
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)), // Set the signing key
                ValidateIssuer = true, // Validate the issuer
                ValidIssuer = Issuer, // Set the valid issuer
                ValidateAudience = true, // Validate the audience
                ValidAudience = "api.example.com", // Set the valid audience
                ValidateLifetime = true // Validate the token's lifetime
            };

            // Validate the token
            claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch(Exception ex)
        {
        }

        var principalID = claimsPrincipal?.FindFirst(ClaimTypes.Name)?.Value ?? "unauthorized";
        var effect = claimsPrincipal == null ? "Deny" : "Allow";

        if (claimsPrincipal != null)
        {
            context.Logger.LogLine($"Token validation succeeded for {principalID}");
        }

        // Extract the IAM response from the token claims
        var iamResponse = new APIGatewayCustomAuthorizerV2IamResponse
        {
            PrincipalID = principalID,
            PolicyDocument = new APIGatewayCustomAuthorizerPolicy
            {
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>
                {
                    new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                    {
                        Action = ["execute-api:Invoke"],
                        Effect = effect,
                        Resource = [request.RouteArn]
                    }
                }
            }
        };

        return iamResponse;
    }
} 