using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AccessToken;

/// <summary>
/// A simple function that generates a JWT token
/// </summary>
public class Function
{
    /// <summary>
    /// Secret key used to sign the JWT token
    /// </summary>
    private readonly string SecretKey;

    /// <summary>
    /// Issuer of the JWT token
    /// </summary>
    private readonly string Issuer;

    /// <summary>
    /// Expiration time of the JWT token in minutes
    /// </summary>
    private readonly int ExpirationMinutes;

    public Function()
    {
        // Get the secret key, issuer, and expiration time from the environment variables
        SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? throw new ArgumentNullException("JWT_SECRET_KEY");
        Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new ArgumentNullException("JWT_ISSUER");
        ExpirationMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES") ?? "5");
    }

    /// <summary>
    /// Lambda function handler for generating a JWT token
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Generate a JWT token
        var token = GenerateJwtToken();

        // Return the token in the response
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = token
        };
    }

    /// <summary>
    /// Generates a JWT token with the specified claims
    /// </summary>
    /// <returns></returns>
    private string GenerateJwtToken()
    {
        // Create the security key
        var key = Encoding.ASCII.GetBytes(SecretKey);
        // Create the token descriptor
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Raymond"),
                new Claim(ClaimTypes.Email, "me@example.com"),
            }),
            Expires = DateTime.UtcNow.AddMinutes(ExpirationMinutes),
            Issuer = Issuer,
            Audience = "api.example.com",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        // Create the JWT token
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
