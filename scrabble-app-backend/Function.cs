using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using System.Text;
using DotNetEnv;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace scrabble_app_backend
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="request">The event for the Lambda function handler to process.</param>
        /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {

            try
            {
                if (string.IsNullOrEmpty(request.Body)) {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = 400,
                        Body = JsonConvert.SerializeObject(new { error = "Request body is null" }),
                        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                    };
                }
                var board = JsonConvert.DeserializeObject<ScrabbleBoardInstance>(request.Body);
                board.specialTiles = board.GetSpecialTiles();
                string response = await GeminiRequest(board);

                

                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonConvert.SerializeObject(response),
                    Headers = new Dictionary<string, string>
                    {
                        {"Content-Type", "application/json" }
                    }
                };
            }
            catch (JsonException e)
            {
                context.Logger.LogLine("Deserialization error: " + e.Message);
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonConvert.SerializeObject(
                        new { error = $"An error occurred while executing the function {e.Message}" }),
                    Headers = new Dictionary<string, string>
                    {
                        {"Content-Type", "application/json" }
                    }
                };
            }

        }

        static async Task<string> GetSecret()
        {
            string secretName = "scrabbleLambdaSecret";
            string region = "us-east-2";

            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT",
            };

            GetSecretValueResponse response;

            try
            {
                response = await client.GetSecretValueAsync(request);
            }
            catch (Exception e)
            {
                // For a list of the exceptions thrown, see
                // https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
                throw e;
            }

            return response.SecretString;
        }


        public async Task<string> GeminiRequest(ScrabbleBoardInstance b)
        {
            var secret = await GetSecret();
            var secretObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(secret);
            var apiKey = secretObj["GEMINI_API_KEY"];

            using (var client = new HttpClient())
            {

                var requestBody = new
                {
                    contents = new[]
                    {
                    new {
                        parts = new[]
                        {
                            new { text = "I have scrabble tiles: a, s, t, r, y, f, & s. Open tiles to play off of are l, q, n, and p. What are the highest point words I can play?" }
                        }
                    }
                }
                };

                string json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}",
                    content
                );

                string responseBody = await response.Content.ReadAsStringAsync();

                dynamic parsed = JsonConvert.DeserializeObject(responseBody);
                string answer = parsed?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString()
                    ?? "No response from Gemini"; return answer;
            }
        }

        
    }

}

