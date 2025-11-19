namespace MonopolyProject
{
    public class Player
    {
        public string Name { get; }

        // Statistics used for the player list command
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; } 
        public int Losses { get; set; } 

        // Current in-game status
        public int Money { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public List<Space> Properties { get; private set; }
        public bool IsInJail { get; set; }
        public int TurnsInJail { get; set; }
        public int DoublesInARow { get; set; }
        public bool IsBankrupt { get; set; }

        public Player(string name)
        {
            Name = name;
            GamesPlayed = 0;
            Wins = 0;
            Draws = 0;
            Losses = 0;
            Properties = new List<Space>();
            ResetForNewGame();
        }

        // Resets the player's state for a fresh game session
        public void ResetForNewGame()
        {
            Money = 600; 
            SetPosition(3, 3); 
            Properties.Clear();
            IsInJail = false;
            TurnsInJail = 0;
            DoublesInARow = 0;
            IsBankrupt = false;
        }

        // Updates the player's coordinates on the board
        public void SetPosition(int x, int y)
        {
            PositionX = x;
            PositionY = y;
        }
    }
}