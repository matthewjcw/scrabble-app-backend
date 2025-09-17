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
            var corsHeaders = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "http://127.0.0.1:5500" },
                { "Access-Control-Allow-Headers", "Content-Type,x-api-key" },
                { "Access-Control-Allow-Methods", "OPTIONS,POST" }
            };

            
            if (request.HttpMethod == "OPTIONS")
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = "",
                    Headers = corsHeaders
                };
            }

            try
            {
                if (string.IsNullOrEmpty(request.Body)) {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = 400,
                        Body = JsonConvert.SerializeObject(new { error = "Request body is null" }),
                        Headers = corsHeaders
                    };
                }
                var boardInstance = JsonConvert.DeserializeObject<ScrabbleBoardInstance>(request.Body);
                boardInstance.specialTiles = boardInstance.GetSpecialTiles();
                string response = await GeminiRequest(boardInstance);

                

                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonConvert.SerializeObject(response),
                    Headers = corsHeaders
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
                    Headers = corsHeaders
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
                            new { text = BuildPrompt(b) }
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

        public string BuildPrompt(ScrabbleBoardInstance b)
        {
            StringBuilder prompt = new StringBuilder();
            prompt.AppendLine("You are helping me find the optimal play in a scrabble game.");
            prompt.AppendLine($"My availible tiles are: {string.Join(", ", b.playerHand)}");
            prompt.AppendLine($"The tiles on the board to play off of are: {string.Join(", ", b.FormatBoard(b.tilesOnBoard))}");
            prompt.AppendLine($"The special tiles on the board are {b.FormatSpecialTiles}\n " +
                $"Also remember that special tiles already covered by a tile don't count for bonus score");
            prompt.AppendLine("Here are the rules: Scrabble is a word game played on a 15x15 board. Each player has a rack of 7 letter tiles. Players take turns forming words on the board using their tiles. Words can be placed horizontally or vertically and must connect to existing tiles. \r\n\r\nScoring rules:\r\n- Each letter has a point value; rare letters like Q and Z are worth more.\r\n- The board has special tiles:\r\n    - Double Letter (DL): doubles the point value of the letter on it.\r\n    - Triple Letter (TL): triples the point value of the letter on it.\r\n    - Double Word (DW): doubles the total score of the word that covers it.\r\n    - Triple Word (TW): triples the total score of the word that covers it.\r\n- Using all 7 tiles in one turn (a \"bingo\") gives a 50-point bonus.\r\n- Words must appear in a standard dictionary; abbreviations, proper nouns, and prefixes/suffixes alone are not allowed.\r\n- New words must connect to previously played words. All formed words in the turn must be valid.\r\n- Players can build off letters already on the board, extending words or creating multiple words in one turn.\r\n- The game ends when all tiles are drawn and one player has used all their tiles, or when no more moves are possible. The player with the highest total score wins.\r\n");
            prompt.AppendLine("In your response, only list off a couple optimal words and where and approx how many points. Very brief, no extra dialogue. The response will be returned in an API call directly, so don't format with \n. Also remember, the given tiles make words horizontally and vertiacally. The words are connected by those tiles I gave. Take your time to find the vertical words on the board. Based on the format it can be difficult");
            prompt.AppendLine($"After this, to test also list of the words already played on the board, and send back what is stored here: {b.FormatBoard(b.tilesOnBoard)}");
            return prompt.ToString();
        }
    }

}

