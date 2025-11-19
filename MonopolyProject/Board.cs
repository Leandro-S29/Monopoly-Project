using System.Text;
using System.Collections;

namespace MonopolyProject
{
    public class Board : IEnumerable<Space> 
    {
        // A 7x7 grid representing the board
        private readonly Space[,] spaces = new Space[7, 7];
        public int FreeParkMoney { get; set; }

        public Board()
        {
            FreeParkMoney = 0;
            InitializeBoard();
        }

        public Space GetSpace(int x, int y)
        {
            return spaces[y, x]; // Accessed as [row, col] -> [y, x]
        }

        // Moves a player, handling the wrap-around logic for board edges
        public Space MovePlayer(Player player, int moveX, int moveY)
        {
            // Movement logic:
            // Vertical: + is Up, - is Down
            // Horizontal: + is Right, - is Left
            
            // Calculate new Y (vertical)
            int newY = player.PositionY - moveY;
            
            // Calculate new X (horizontal)
            int newX = player.PositionX + moveX;

            // Apply wrap-around to keep coordinates within the 0-6 range
            newX = (newX % 7 + 7) % 7;
            newY = (newY % 7 + 7) % 7;

            player.SetPosition(newX, newY);
            return GetSpace(newX, newY);
        }

        // Generates a string representation of the board state
        public string GetBoardDetails(List<Player> players)
        {
            // 1. Create a grid of strings to hold cell content
            string[,] cellContents = new string[7, 7];
            int[] colWidths = new int[7];

            for (int y = 0; y < 7; y++)
            {
                for (int x = 0; x < 7; x++)
                {
                    Space space = GetSpace(x, y);
                    var content = new StringBuilder();
                    content.Append(space.Name);

                    // Add owner and house info if applicable
                    if (space.Owner != null)
                    {
                        content.Append($" ({space.Owner.Name}");
                        if (space.HouseCount > 0)
                        {
                            content.Append($"-{space.HouseCount}");
                        }
                        content.Append(")");
                    }

                    // Show players currently on this space
                    foreach (var p in players)
                    {
                        // Special display logic for Prison (Just Visiting vs In Jail)
                        if (space.Type == SpaceType.Prison)
                        {
                            if (p.IsInJail)
                            {
                                content.Append($" ({p.Name})"); // In Jail
                            }
                            else if (p.PositionX == x && p.PositionY == y)
                            {
                                content.Append($" {p.Name}"); // Just Visiting
                            }
                        }
                        else if (p.PositionX == x && p.PositionY == y && !p.IsInJail)
                        {
                            content.Append($" {p.Name}");
                        }
                    }

                    cellContents[y, x] = content.ToString();
                    // Track max column width for padding
                    if (cellContents[y, x].Length > colWidths[x])
                    {
                        colWidths[x] = cellContents[y, x].Length;
                    }
                }
            }

            // 2. Build the final formatted string
            var boardString = new StringBuilder();
            for (int y = 0; y < 7; y++)
            {
                for (int x = 0; x < 7; x++)
                {
                    boardString.Append(cellContents[y, x].PadRight(colWidths[x] + 2)); // Add spacing
                }
                boardString.AppendLine();
            }
            return boardString.ToString();
        }

        // Sets up the initial state of the 49 board spaces
        private void InitializeBoard()
        {
            // Row 0
            spaces[0, 0] = new Space("Prison", SpaceType.Prison, ColorGroup.None, 0);
            spaces[0, 1] = new Space("Green3", SpaceType.Property, ColorGroup.Green, 160);
            spaces[0, 2] = new Space("Violet1", SpaceType.Property, ColorGroup.Violet, 150);
            spaces[0, 3] = new Space("Train2", SpaceType.Train, ColorGroup.None, 150);
            spaces[0, 4] = new Space("Red3", SpaceType.Property, ColorGroup.Red, 160);
            spaces[0, 5] = new Space("White1", SpaceType.Property, ColorGroup.White, 160);
            spaces[0, 6] = new Space("BackToStart", SpaceType.BackToStart, ColorGroup.None, 0);

            // Row 1
            spaces[1, 0] = new Space("Blue3", SpaceType.Property, ColorGroup.Blue, 170);
            spaces[1, 1] = new Space("Community", SpaceType.Community, ColorGroup.None, 0);
            spaces[1, 2] = new Space("Red2", SpaceType.Property, ColorGroup.Red, 130);
            spaces[1, 3] = new Space("Violet2", SpaceType.Property, ColorGroup.Violet, 130);
            spaces[1, 4] = new Space("Water Works", SpaceType.Utility, ColorGroup.None, 120);
            spaces[1, 5] = new Space("Chance", SpaceType.Chance, ColorGroup.None, 0);
            spaces[1, 6] = new Space("White2", SpaceType.Property, ColorGroup.White, 180);

            // Row 2
            spaces[2, 0] = new Space("Blue2", SpaceType.Property, ColorGroup.Blue, 140);
            spaces[2, 1] = new Space("Red1", SpaceType.Property, ColorGroup.Red, 130);
            spaces[2, 2] = new Space("Chance", SpaceType.Chance, ColorGroup.None, 0);
            spaces[2, 3] = new Space("Brown2", SpaceType.Property, ColorGroup.Brown, 120);
            spaces[2, 4] = new Space("Community", SpaceType.Community, ColorGroup.None, 0);
            spaces[2, 5] = new Space("Black1", SpaceType.Property, ColorGroup.Black, 110);
            spaces[2, 6] = new Space("Lux Tax", SpaceType.LuxTax, ColorGroup.None, 80); // Price is the tax amount

            // Row 3
            spaces[3, 0] = new Space("Train1", SpaceType.Train, ColorGroup.None, 150);
            spaces[3, 1] = new Space("Green2", SpaceType.Property, ColorGroup.Green, 140);
            spaces[3, 2] = new Space("Teal1", SpaceType.Property, ColorGroup.Teal, 90);
            spaces[3, 3] = new Space("Start", SpaceType.Start, ColorGroup.None, 0);
            spaces[3, 4] = new Space("Teal2", SpaceType.Property, ColorGroup.Teal, 130);
            spaces[3, 5] = new Space("Black2", SpaceType.Property, ColorGroup.Black, 120);
            spaces[3, 6] = new Space("Train3", SpaceType.Train, ColorGroup.None, 150);

            // Row 4
            spaces[4, 0] = new Space("Blue1", SpaceType.Property, ColorGroup.Blue, 140);
            spaces[4, 1] = new Space("Green1", SpaceType.Property, ColorGroup.Green, 120);
            spaces[4, 2] = new Space("Community", SpaceType.Community, ColorGroup.None, 0);
            spaces[4, 3] = new Space("Brown1", SpaceType.Property, ColorGroup.Brown, 100);
            spaces[4, 4] = new Space("Chance", SpaceType.Chance, ColorGroup.None, 0);
            spaces[4, 5] = new Space("Black3", SpaceType.Property, ColorGroup.Black, 130);
            spaces[4, 6] = new Space("White3", SpaceType.Property, ColorGroup.White, 190);

            // Row 5
            spaces[5, 0] = new Space("Pink1", SpaceType.Property, ColorGroup.Pink, 160);
            spaces[5, 1] = new Space("Chance", SpaceType.Chance, ColorGroup.None, 0);
            spaces[5, 2] = new Space("Orange1", SpaceType.Property, ColorGroup.Orange, 120);
            spaces[5, 3] = new Space("Orange2", SpaceType.Property, ColorGroup.Orange, 120);
            spaces[5, 4] = new Space("Orange3", SpaceType.Property, ColorGroup.Orange, 140);
            spaces[5, 5] = new Space("Community", SpaceType.Community, ColorGroup.None, 0);
            spaces[5, 6] = new Space("Yellow3", SpaceType.Property, ColorGroup.Yellow, 170);

            // Row 6
            spaces[6, 0] = new Space("FreePark", SpaceType.FreePark, ColorGroup.None, 0);
            spaces[6, 1] = new Space("Pink2", SpaceType.Property, ColorGroup.Pink, 180);
            spaces[6, 2] = new Space("Electric Company", SpaceType.Utility, ColorGroup.None, 120);
            spaces[6, 3] = new Space("Train4", SpaceType.Train, ColorGroup.None, 150);
            spaces[6, 4] = new Space("Yellow1", SpaceType.Property, ColorGroup.Yellow, 140);
            spaces[6, 5] = new Space("Yellow2", SpaceType.Property, ColorGroup.Yellow, 140);
            spaces[6, 6] = new Space("Police", SpaceType.Police, ColorGroup.None, 0);
        }

        // Enables iteration over the internal array
        public IEnumerator<Space> GetEnumerator()
        {
            foreach (var space in spaces)
            {
                yield return space;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}