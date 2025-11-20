using System;

namespace MonopolyProject.Models
{
    public class Player
    {
        // Registered Data
        public string Name { get; private set; }
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }

        // In-Game Data
        public int Money { get; set; }
        public int Row { get; set; } // Y position
        public int Col { get; set; } // X position
        public bool IsInJail { get; set; }
        public int TurnsInJail { get; set; } // Counts number of turns in jail
        public bool HasRolledThisTurn { get; set; }
        public bool NeedsToPayRent { get; set; } // To Lock turn if rent is due
        public int DoublesCount { get; set; } // To track consecutive doubles dice rolls

        public Player(string name)
        {
            Name = name;
            GamesPlayed = 0;
            Wins = 0;
            Draws = 0;
            Losses = 0;
        }

        public void ResetForGame()
        {
            Money = 600; 
            Row = 3; // Center of 7x7
            Col = 3; // Center of 7x7
            IsInJail = false;
            TurnsInJail = 0;
            HasRolledThisTurn = false;
            NeedsToPayRent = false;
            DoublesCount = 0;
        }
    }
}