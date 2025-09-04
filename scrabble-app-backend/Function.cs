using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

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
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                ScrabbleBoardInstance board = JsonConvert.DeserializeObject<ScrabbleBoardInstance>(request.Body);
            }
            catch (JsonException e)
            {

            }

        }
    }

    public class ScrabbleBoardInstance
    {
        public Dictionary<string, int[][]>? specialTiles;

        public char?[] playerHand = new char?[7];

        public char[][]? tilesOnBoard;

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
    }
}
