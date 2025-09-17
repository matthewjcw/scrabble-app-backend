using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scrabble_app_backend;

namespace scrabble_app_backend
{
    
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


        public string FormatSpecialTiles(Dictionary<string, int[][]> specialTiles)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Special tiles on the board:");

            foreach (var kvp in specialTiles)
            {
                sb.Append($"{kvp.Key}: ");
                var coords = kvp.Value.Select(coord => $"({coord[0]},{coord[1]})");
                sb.AppendLine(string.Join(", ", coords));
            }

            return sb.ToString();
        }

        public string FormatBoard(char?[][] board)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Current Scrabble board (15x15):");

            for (int row = 0; row < board.Length; row++)
            {
                for (int col = 0; col < board[row].Length; col++)
                {
                    // Show the letter if present, otherwise a dot
                    char display = board[row][col] ?? '.';
                    sb.Append(display).Append(' ');
                }
                sb.AppendLine(); // new line at the end of the row
            }

            return sb.ToString();
        }
    }
    
}
