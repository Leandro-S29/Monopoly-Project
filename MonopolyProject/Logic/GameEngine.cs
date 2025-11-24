using System;
using System.Collections.Generic;
using System.Diagnostics;
using MonopolyProject.Models;

namespace MonopolyProject.Logic
{
    public class GameEngine
    {
        // Game state variables
        private bool gameInProgress;
        private int freeParkingFunds;

        // The game board instance and random number generator
        private Board board;
        private Random random;

        // To track the current player's turn
        private int currentPlayerIndex;

        // Player management
        private Dictionary<string, Player> registeredPlayers = new Dictionary<string, Player>();
        private List<Player> activePlayersInGameWithOrder = new List<Player>();

        public GameEngine()
        {
            registeredPlayers = new Dictionary<string, Player>();
            activePlayersInGameWithOrder = new List<Player>();
            gameInProgress = false;
            random = new Random();
            freeParkingFunds = 0;
        }


        // Method to read and process commands
        public void commandReading(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            string[] commandParts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (commandParts.Length == 0)
            {
                return;
            }

            string commandType = commandParts[0].ToUpper();

            switch (commandType)
            {
                case "RJ":
                    RegisterPlayer(commandParts);
                    break;
                case "LJ":
                    ListPlayers();
                    break;
                case "IJ":
                    StartGame(commandParts);
                    break;
                case "LD":
                    RollDice(commandParts);
                    break;
                case "CE":
                    BuySpace(commandParts);
                    break;
                case "DJ":
                    GameDetails();
                    break;
                case "TT":
                    EndTurn(commandParts);
                    break;
                case "PA":
                    PayRent(commandParts);
                    break;
                case "CC":
                    BuyHouse(commandParts);
                    break;
                case "TC":
                    TakeCard(commandParts);
                    break;
                default:
                    Console.WriteLine("Instrução inválida.");
                    break;
            }

        }

        // Method to register a new player
        public void RegisterPlayer(string[] commandParts)
        {
            if (commandParts.Length != 2) { Console.WriteLine("Instrução inválida."); return; }
            string name = commandParts[1];

            if (registeredPlayers.ContainsKey(name))
            {
                Console.WriteLine("Jogador existente.");
                return;
            }
            
            registeredPlayers.Add(name, new Player(name));
            Console.WriteLine("Jogador registado com sucesso.");
        }

        // Method to list all registered players
        public void ListPlayers()
        {
            if (registeredPlayers.Count == 0)
            {
                Console.WriteLine("Sem jogadores registados.");
                return;
            }

            // Sort players by wins descending, then by name ascending using a List
            var players = new List<Player>(registeredPlayers.Values);
            players.Sort((a, b) =>
            {
                int winComparison = b.Wins.CompareTo(a.Wins);
                return winComparison != 0 ? winComparison : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });

            foreach (var player in players)
            {
                Console.WriteLine($"{player.Name} {player.GamesPlayed} {player.Wins} {player.Draws} {player.Losses}");
            }
        }

        // Method to start a new game
        public void StartGame(string[] commandParts)
        {
            if (commandParts.Length != 5)
            {
                Console.WriteLine("Instrução inválida.");
                return;
            }
            else if (gameInProgress == true)
            {
                Console.WriteLine("Existe um jogo em curso.");
                return;
            }

            List<string> playerNames = new List<string>();
            for (int i = 1; i < commandParts.Length; i++)
            {
                playerNames.Add(commandParts[i]);
            }

            // Clear previous game state
            activePlayersInGameWithOrder.Clear();

            foreach (string playerName in playerNames)
            {
                // Check if player is NOT registered
                if (!registeredPlayers.ContainsKey(playerName))
                {
                    Console.WriteLine("Jogador inexistente.");
                    return;
                }

                Player player = registeredPlayers[playerName];

                // Check if player is already in the game
                if (activePlayersInGameWithOrder.Contains(player))
                {
                    Console.WriteLine("Jogador inexistente.");
                    return;
                }

                // Add player to the active game list
                activePlayersInGameWithOrder.Add(player);
            }

            freeParkingFunds = 0;
            board = new Board();


            foreach (var player in activePlayersInGameWithOrder)
            {
                player.ResetForGame();
                player.GamesPlayed++;
            }

            currentPlayerIndex = 0;
            gameInProgress = true;
            Console.WriteLine("Jogo iniciado com sucesso.");
        }

        public void RollDice(string[] commandParts)
        {
            if (commandParts.Length != 2 && commandParts.Length != 4)
            {
                Console.WriteLine("Instrução inválida.");
                return;
            }

            string playerName = commandParts[1];
            
            // Check if game is in progress
            if (!gameInProgress)
            {
                Console.WriteLine("Não existe um jogo em curso.");
                return;
            }

            Player activePlayer = GetActivePlayer(playerName);

            // Check if player is in the current game
            if (activePlayer == null)
            {
                Console.WriteLine("Jogador não participa no jogo em curso.");
                return;
            }

            // Check if it's the player's turn using the list index
            if (activePlayer != activePlayersInGameWithOrder[currentPlayerIndex])
            {
                Console.WriteLine("Não é a vez do jogador.");
                return;
            }

            // Check if player has already rolled and has no doubles to continue
            if (activePlayer.HasRolledThisTurn && activePlayer.DoublesCount == 0)
            {
                return;
            }

            // Check if player needs to pay rent or take card before rolling
            if (activePlayer.NeedsToPayRent || activePlayer.HasCommunityOrChanceCard)
            {
                return;
            }

            // Roll the dice (Cheat Mode or Random)
            int d1, d2;

            if (commandParts.Length == 4)
            {
                // Attempt to parse the cheat values
                if (!int.TryParse(commandParts[2], out d1) || !int.TryParse(commandParts[3], out d2))
                {
                    Console.WriteLine("Instrução inválida.");
                    return;
                }
            }
            else
            {
                // Standard logic: Generate random values between -3 and 3, excluding 0
                do { d1 = random.Next(-3, 4); } while (d1 == 0);
                do { d2 = random.Next(-3, 4); } while (d2 == 0);
            }

            bool isDouble = (d1 == d2); // Check for doubles

            if (activePlayer.IsInJail)
            {
                if (isDouble || activePlayer.TurnsInJail >= 3)
                {
                    activePlayer.IsInJail = false;
                    activePlayer.TurnsInJail = 0;
                    // Proceed to move
                }
                else
                {
                    activePlayer.TurnsInJail++;
                    activePlayer.HasRolledThisTurn = true;
                    Console.WriteLine($"Saiu {d1}/{d2} - espaço Prison. Jogador só de passagem.");
                    return;
                }
            }

            // Handle Movement
            // d1 = Horizontal (Right +, Left -)
            // d2 = Vertical (Up +, Down -)
            int moveRow = -d2;
            int moveCol = d1;

            int newRow = activePlayer.Row + moveRow;
            int newCol = activePlayer.Col + moveCol;

            // Handle board wrapping : Movements continues on the opposite side
            while (newRow < 0) newRow += 7;
            while (newRow > 6) newRow -= 7;
            while (newCol < 0) newCol += 7;
            while (newCol > 6) newCol -= 7;

            activePlayer.Row = newRow;
            activePlayer.Col = newCol;
            activePlayer.HasRolledThisTurn = true;

            Space spacing = board.Grid[activePlayer.Row, activePlayer.Col];

            if (isDouble)
            {
                activePlayer.DoublesCount++;
                activePlayer.HasRolledThisTurn = false; // Allow rolling again
                if (activePlayer.DoublesCount == 2) // If happens 2 in a row 
                {
                    //Players goes to jail
                    GoToJail(activePlayer);
                    Console.WriteLine($"Saiu {d1}/{d2} - espaço Police. Jogador preso.");
                    return;
                }
            }
            else
            {
                activePlayer.DoublesCount = 0;
            }

            Console.WriteLine($"Saiu {d1}/{d2} -");

            HandleSpaceInteraction(activePlayer, spacing);
        }

        // Helper methods for RollDice
        // Property handler
        private void HandleProperty(Player player, Space space)
        {
            if (space.Owner == null)
            {
                Console.WriteLine($"espaço {space.Name}. Espaço sem dono.");
            }
            else if (space.Owner == player)
            {
                Console.WriteLine($"espaço {space.Name}. Espaço já comprado.");
            }
            else
            {
                Console.WriteLine($"espaço {space.Name}. Espaço já comprado por outro jogador. Necessário pagar renda.");
                player.NeedsToPayRent = true;
            }
        }

        // Special spaces handler
        private void HandleSpecialSpaces(Player player, Space space)
        {
            switch (space.Type)
            {
                case SpaceType.GoToStart:
                    Console.WriteLine($"Espaço BackToStart. Peça colocada no espaço Start.");
                    player.Row = 3; player.Col = 3;
                    player.Money += 200;
                    break;
                case SpaceType.Police:
                    Console.WriteLine($"espaço Police. Jogador preso.");
                    GoToJail(player);
                    break;
                case SpaceType.Prison:
                    Console.WriteLine($"espaço Prison. Jogador só de passagem.");
                    break;
                case SpaceType.FreePark:
                    Console.WriteLine($"espaço FreePark. Jogador recebe {freeParkingFunds} ValorGuardado No Free Park.");
                    player.Money += freeParkingFunds;
                    freeParkingFunds = 0;
                    break;
            }
        }

        // Space interaction handler
        private void HandleSpaceInteraction(Player player, Space space)
        {
            if (space.Type == SpaceType.Start)
            {
                player.Money += 200;
                return;
            }

            if (space.Type == SpaceType.Street || space.Type == SpaceType.Train || space.Type == SpaceType.Utility)
            {
                HandleProperty(player, space);
            }
            else if (space.Type == SpaceType.Chance || space.Type == SpaceType.Community)
            {
                Console.WriteLine($"espaço {space.Name}. Espaço especial. Tirar carta.");
                player.HasCommunityOrChanceCard = true;
            }
            else if (space.Type == SpaceType.Tax)
            {
                HandleTax(player, space);
            }
            else
            {
                HandleSpecialSpaces(player, space);
            }
        }

        // Tax handling
        private void HandleTax(Player player, Space space)
        {
            if (player.Money >= 80)
            {
                player.Money -= 80;
                freeParkingFunds += 80;
            }
            else
            {
                PlayerBankrupt(player);
            }
        }

        // Other commands
        public void BuySpace(string[] parts)
        {
            //TODO:
        }

        public void GameDetails()
        {
            //TODO:
        }

        public void EndTurn(string[] parts)
        {
            if (parts.Length != 2)
            {
                Console.WriteLine("Instrução inválida.");
                return;
            }
            
            if (!gameInProgress)
            {
                Console.WriteLine("Não existe jogo em curso.");
                return;
            }
            
            string playerName = parts[1];
            Player activePlayer = GetActivePlayer(playerName);

            if(activePlayer != activePlayersInGameWithOrder[currentPlayerIndex])
            {
                Console.WriteLine("Não é o turno do jogador indicado.");
                return;
            }

            if (!activePlayer.HasRolledThisTurn || activePlayer.NeedsToPayRent || activePlayer.HasCommunityOrChanceCard || (activePlayer.DoublesCount > 0 && activePlayer.DoublesCount < 2))
            {
                Console.WriteLine("jogador ainda tem ações a fazer.");
                return;
            }

            // Reset player state for next turn
            activePlayer.HasRolledThisTurn = false;
            activePlayer.DoublesCount = 0;
            activePlayer.NeedsToPayRent = false;
            activePlayer.HasCommunityOrChanceCard = false;

            // Move to next player
            currentPlayerIndex = (currentPlayerIndex + 1) % activePlayersInGameWithOrder.Count;

            String nextPlayerName = activePlayersInGameWithOrder[currentPlayerIndex].Name;
            Console.WriteLine($"Turno terminado. Novo turno do jogador {nextPlayerName}.");    

        }

        public void PayRent(string[] parts)
        {
            //TODO:    
        }

        public void BuyHouse(string[] parts)
        {
            // TODO:
        }

        public void TakeCard(string[] parts)
        {
            // TODO:
        }


        // Helpers Methods
        public void GoToJail(Player player)
        {
            // Moves player to prison
            player.Row = 0; // Makes player go to row = 0
            player.Col = 0; // Makes player go to col = 0

            player.IsInJail = true;
            player.TurnsInJail = 0; // Resets turns in jail counter
            player.HasRolledThisTurn = true; // Locks player from rolling dice this turn
        }

        public Player GetActivePlayer(String playerName)
        {
            // Iterate list to find player (replacement for Dictionary lookup)
            foreach (var player in activePlayersInGameWithOrder)
            {
                if (player.Name == playerName)
                {
                    return player;
                }
            }
            return null;
        }

        public void PlayerBankrupt(Player player)
        {
            // Remove Player from the game using the list
            int playerIndex = -1;
            for (int i = 0; i < activePlayersInGameWithOrder.Count; i++)
            {
                if (activePlayersInGameWithOrder[i] == player)
                {
                    playerIndex = i;
                    break;
                }
            }

            if (playerIndex != -1)
            {
                activePlayersInGameWithOrder.RemoveAt(playerIndex);

                // If the player removed was before the current player, decrement index
                // to keep the turn with the correct next person
                if (playerIndex < currentPlayerIndex)
                {
                    currentPlayerIndex--;
                }
            }

            // Handle wrap around if index is now out of bounds
            if (currentPlayerIndex >= activePlayersInGameWithOrder.Count)
            {
                currentPlayerIndex = 0;
            }

            // update the player's stats 
            player.Losses += 1;
            if (activePlayersInGameWithOrder.Count == 1)
            {
                gameInProgress = false;
                activePlayersInGameWithOrder[0].Wins += 1;
            }
        }

    }
}