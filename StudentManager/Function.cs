using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StudentManager;

public class Function
{
    /// <summary>
    /// Handles API Gateway requests to get or create students.
    /// </summary>
    /// <param name="request">The API Gateway request.</param>
    /// <param name="context">The Lambda context.</param>
    /// <returns>The API Gateway response.</returns>
    public static async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        Console.WriteLine(JsonSerializer.Serialize(request));
        AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        DynamoDBContext dbContext = new DynamoDBContext(client);
        if (request.RouteKey.Contains("GET /"))
        {
            var data = await dbContext.ScanAsync<Student>(default).GetRemainingAsync();
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonSerializer.Serialize(data),
                StatusCode = 200
            };
        }
        else if (request.RouteKey.Contains("POST /") && request.Body != null)
        {
            var newStudent = JsonSerializer.Deserialize<Student>(request.Body);
            await dbContext.SaveAsync(newStudent);
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = $"Student with Id {newStudent?.Id} Created",
                StatusCode = 201
            };
        }
        else
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = "Bad Request",
                StatusCode = 400
            };
        }
    }
}
