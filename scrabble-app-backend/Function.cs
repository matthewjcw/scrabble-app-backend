using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using System.Text;

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
                ScrabbleBoardInstance board = JsonConvert.DeserializeObject<ScrabbleBoardInstance>(request.Body);
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


        public async Task<string> GeminiRequest(ScrabbleBoardInstance b)
        {
            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-goog-api-key", apiKey);

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
                    "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent",
                    content
                );

                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Raw Response:");
                Console.WriteLine(responseBody);

                // Try extracting the text field like your JS code did
                dynamic parsed = JsonConvert.DeserializeObject(responseBody);
                string answer = parsed?.candidates?[0]?.content?.parts?[0]?.text ?? "No response from Gemini";

                Console.WriteLine("\nGemini Answer:");
                Console.WriteLine(answer);
                return answer;
            }
        }

        public class ScrabbleBoardInstance
        {
            public Dictionary<string, int[][]>? specialTiles;

            public char?[] playerHand = new char?[7];

            public char?[][]? tilesOnBoard;

            public Dictionary<string, int[][]> GetSpecialTiles()
            {
                return new Dictionary<string, int[][]>()
                {
                    ["double-word"] = new int[][]
        {
        new[] {1, 1}, new[] {2, 2}, new[] {3, 3}, new[] {4, 4}, new[] {10, 10}, new[] {11, 11}, new[] {12, 12}, new[] {13, 13},
        new[] {1, 13}, new[] {2, 12}, new[] {3, 11}, new[] {4, 10}, new[] {10, 4}, new[] {11, 3}, new[] {12, 2}, new[] {13, 1},
        new[] {7, 0}, new[] {0, 7}, new[] {14, 7}, new[] {7, 14}
        },
                    ["triple-word"] = new int[][]
        {
        new[] {0, 0}, new[] {7, 0}, new[] {14, 0}, new[] {0, 7}, new[] {14, 7}, new[] {0, 14}, new[] {7, 14}, new[] {14, 14}
        },
                    ["double-letter"] = new int[][]
        {
        new[] {3, 0}, new[] {11, 0}, new[] {6, 2}, new[] {8, 2}, new[] {0, 3}, new[] {7, 3}, new[] {14, 3}, new[] {2, 6},
        new[] {6, 6}, new[] {8, 6}, new[] {12, 6}, new[] {3, 7}, new[] {11, 7}, new[] {2, 8}, new[] {6, 8}, new[] {8, 8},
        new[] {12, 8}, new[] {0, 11}, new[] {7, 11}, new[] {14, 11}, new[] {6, 12}, new[] {8, 12}, new[] {3, 14}, new[] {11, 14}
        },
                    ["triple-letter"] = new int[][]
        {
        new[] {1, 5}, new[] {5, 1}, new[] {5, 5}, new[] {9, 1}, new[] {9, 5}, new[] {13, 5}, new[] {1, 9}, new[] {5, 9},
        new[] {5, 13}, new[] {9, 9}, new[] {9, 13}, new[] {13, 9}
        },
                    ["center"] = new int[][]
        {
        new[] {7, 7}
        }
                };
            }

            public string FormatScrabbleBoard(char?[][]? tilesOnBoard)
            {
                if (tilesOnBoard == null) return "(empty board)";

                var rows = new List<string>();

                foreach (var row in tilesOnBoard)
                {
                    if (row == null)
                    {
                        rows.Add(new string('.', 15)); // default 15 wide scrabble row
                        continue;
                    }

                    var chars = row.Select(c => c.HasValue ? c.Value : '.').ToArray();
                    rows.Add(new string(chars));
                }

                return string.Join("\n", rows);
            }
        }
    }
}
