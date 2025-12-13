using System;
using System.Collections.Generic;

namespace MonopolyProject.Models
{
    public class Board
    {
        public Space[,] Grid { get; private set; }

        public Board()
        {
            Grid = new Space[7, 7];
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            // Helper to initialize the board with spaces

            // Row 0
            Grid[0, 0] = new Space("Prison", SpaceType.Prison, 0, ColorType.Blank);
            Grid[0, 1] = new Space("Green3", SpaceType.Street, 160, ColorType.Green);
            Grid[0, 2] = new Space("Violet1", SpaceType.Street, 150, ColorType.Violet);
            Grid[0, 3] = new Space("Train2", SpaceType.Train, 150, ColorType.Utility);
            Grid[0, 4] = new Space("Red3", SpaceType.Street, 160, ColorType.Red);
            Grid[0, 5] = new Space("White1", SpaceType.Street, 160, ColorType.White);
            Grid[0, 6] = new Space("BackToStart", SpaceType.GoToStart, 0, ColorType.Blank);

            // Row 1
            Grid[1, 0] = new Space("Blue3", SpaceType.Street, 170, ColorType.Blue);
            Grid[1, 1] = new Space("Community", SpaceType.Community, 0, ColorType.Blank);
            Grid[1, 2] = new Space("Red2", SpaceType.Street, 130, ColorType.Red);
            Grid[1, 3] = new Space("Violet2", SpaceType.Street, 130, ColorType.Violet);
            Grid[1, 4] = new Space("Water Works", SpaceType.Utility, 120, ColorType.Blank);
            Grid[1, 5] = new Space("Chance", SpaceType.Chance, 0, ColorType.Blank);
            Grid[1, 6] = new Space("White2", SpaceType.Street, 180, ColorType.White);

            // Row 2
            Grid[2, 0] = new Space("Blue2", SpaceType.Street, 140, ColorType.Blue);
            Grid[2, 1] = new Space("Red1", SpaceType.Street, 130, ColorType.Red);
            Grid[2, 2] = new Space("Chance", SpaceType.Chance, 0, ColorType.Blank);
            Grid[2, 3] = new Space("Brown2", SpaceType.Street, 120, ColorType.Brown);
            Grid[2, 4] = new Space("Community", SpaceType.Community, 0, ColorType.Blank);
            Grid[2, 5] = new Space("Black1", SpaceType.Street, 110, ColorType.Black);
            Grid[2, 6] = new Space("Lux Tax", SpaceType.Tax, 0, ColorType.Blank); // Pays 80 to FreePark

            // Row 3
            Grid[3, 0] = new Space("Train1", SpaceType.Train, 150, ColorType.Utility);
            Grid[3, 1] = new Space("Green2", SpaceType.Street, 140, ColorType.Green);
            Grid[3, 2] = new Space("Teal1", SpaceType.Street, 90, ColorType.Teal);
            Grid[3, 3] = new Space("Start", SpaceType.Start, 0, ColorType.Blank);
            Grid[3, 4] = new Space("Teal2", SpaceType.Street, 130, ColorType.Teal);
            Grid[3, 5] = new Space("Black2", SpaceType.Street, 120, ColorType.Black);
            Grid[3, 6] = new Space("Train3", SpaceType.Train, 150, ColorType.Utility);

            // Row 4
            Grid[4, 0] = new Space("Blue1", SpaceType.Street, 140, ColorType.Blue);
            Grid[4, 1] = new Space("Green1", SpaceType.Street, 120, ColorType.Green);
            Grid[4, 2] = new Space("Community", SpaceType.Community, 0, ColorType.Blank);
            Grid[4, 3] = new Space("Brown1", SpaceType.Street, 100, ColorType.Brown);
            Grid[4, 4] = new Space("Chance", SpaceType.Chance, 0, ColorType.Blank);
            Grid[4, 5] = new Space("Black3", SpaceType.Street, 130, ColorType.Black);
            Grid[4, 6] = new Space("White3", SpaceType.Street, 190, ColorType.White);

            // Row 5
            Grid[5, 0] = new Space("Pink1", SpaceType.Street, 160, ColorType.Pink);
            Grid[5, 1] = new Space("Chance", SpaceType.Chance, 0, ColorType.Blank);
            Grid[5, 2] = new Space("Orange1", SpaceType.Street, 120, ColorType.Orange);
            Grid[5, 3] = new Space("Orange2", SpaceType.Street, 120, ColorType.Orange);
            Grid[5, 4] = new Space("Orange3", SpaceType.Street, 140, ColorType.Orange);
            Grid[5, 5] = new Space("Community", SpaceType.Community, 0, ColorType.Blank);
            Grid[5, 6] = new Space("Yellow3", SpaceType.Street, 170, ColorType.Yellow);

            // Row 6
            Grid[6, 0] = new Space("FreePark", SpaceType.FreePark, 0, ColorType.Blank);
            Grid[6, 1] = new Space("Pink2", SpaceType.Street, 180, ColorType.Pink);
            Grid[6, 2] = new Space("Electric Company", SpaceType.Utility, 120, ColorType.Utility);
            Grid[6, 3] = new Space("Train4", SpaceType.Train, 150, ColorType.Utility);
            Grid[6, 4] = new Space("Yellow1", SpaceType.Street, 140, ColorType.Yellow);
            Grid[6, 5] = new Space("Yellow2", SpaceType.Street, 140, ColorType.Yellow);
            Grid[6, 6] = new Space("Police", SpaceType.Police, 0, ColorType.Blank);
        }

        // TODO: Must Check with the file if its properly done
        public void DisplayBoard(List<Player> players)
        {
            for (int row = 0; row < 7; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    Space space = Grid[row, col];
                    string spaceString = "| " + space.Name;

                    // In case of a player be in jail, show that he is in prison
                    if (space.Type == SpaceType.Prison)
                    {
                        foreach (var p in players)
                        {
                            if (p.IsInJail)
                            {
                                spaceString += $" ({p.Name})";
                            }
                        }
                    }
                    else
                    {
                        // If the space has an owner, show the owner's name
                        if (space.Owner != null)
                        {
                            spaceString += $" ({space.Owner.Name}";
                            // Indicate number of houses if any
                            if (space.HouseCount > 0)
                            {
                                spaceString += $"-{space.HouseCount}";
                            }
                            spaceString += ")";
                        }
                    }

                    // Show players on the space 
                    foreach (var player in players)
                    {
                        if (player.Col == col && player.Row == row)
                        {
                            // Players out of jail are shown on the prison space
                            if (space.Type != SpaceType.Prison || !player.IsInJail)
                            {
                                spaceString += $" {player.Name}";
                            }
                        }
                    }

                    spaceString += " ";
                    Console.Write(spaceString);
                }

                Console.WriteLine("|");
            }
        }
    }
}