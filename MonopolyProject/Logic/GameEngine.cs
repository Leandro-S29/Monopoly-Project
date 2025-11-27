using System;
using System.Collections.Generic;
using System.Diagnostics;
using MonopolyProject.Models;


//FIXME: Refactor to separate into a CLI and Game Logic 

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
        
            var players = new List<Player>(registeredPlayers.Values);
        
            // Sort players by wins (descending) and then by name (ascending)
            for (int i = 0; i < players.Count - 1; i++)
            {
                for (int j = i + 1; j < players.Count; j++)
                {
                    var left = players[i];
                    var right = players[j];
        
                    bool shouldSwap = right.Wins > left.Wins ||
                                      (right.Wins == left.Wins && string.Compare(right.Name, left.Name, StringComparison.Ordinal) < 0);
        
                    if (shouldSwap)
                    {
                        players[i] = right;
                        players[j] = left;
                    }
                }
            }
        
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
                Console.WriteLine("Não é a vez do jogador.");
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
                // Generate random values between -3 and 3, excluding 0
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
            if(parts.Length != 2)
            {
                Console.WriteLine("Instrução inválida.");
                return;
            }
            
            if(!gameInProgress)
            {
                Console.WriteLine("Não existe um jogo em curso.");
                return;
            }

            string playerName = parts[1];
            Player activePlayer = GetActivePlayer(playerName);
            
            if(activePlayer == null)
            {
                Console.WriteLine("Jogador não participa no jogo em curso.");
                return;
            }

            if(activePlayer != activePlayersInGameWithOrder[currentPlayerIndex])
            {
                Console.WriteLine("Não é a vez do jogador.");
                return;
            }
            Space space = board.Grid[activePlayer.Row, activePlayer.Col];

            if(space.Type != SpaceType.Street && space.Type != SpaceType.Train && space.Type != SpaceType.Utility)
            {
                Console.WriteLine("Este espaço não está para venda");
                return;
            }

            if(activePlayer != activePlayersInGameWithOrder[currentPlayerIndex])
            {
                Console.WriteLine("Não é a vez do jogador.");
                return;
            }

            if(space.Owner != null)
            {
                Console.WriteLine("O espaço já se encontra comprado.");
                return;
            }

            
            activePlayer.Money -= space.Price;
            space.Owner = activePlayer;
            Console.WriteLine("Espaço comprado.");
        }

        public void GameDetails()
        {
            if(gameInProgress)
            {
                Player player = GetActivePlayer(activePlayersInGameWithOrder[currentPlayerIndex].Name);
                board.DisplayBoard(activePlayersInGameWithOrder);
                Console.WriteLine($"{player.Name} - {player.Money}");
                return;
            }else
            Console.WriteLine("Não existe nenhum jogo em curso!");
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
            if(parts.Length != 2)
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
                Console.WriteLine("Não é o vez do jogador.");
                return;
            }

            Space currentSpace = board.Grid[activePlayer.Row, activePlayer.Col];
            if (!activePlayer.NeedsToPayRent || currentSpace.Owner == null || currentSpace.Owner == activePlayer || currentSpace.Type != SpaceType.Street && currentSpace.Type != SpaceType.Train && currentSpace.Type != SpaceType.Utility)
            {
                Console.WriteLine("Não é necessário pagar aluguer."); 
                activePlayer.NeedsToPayRent = false; // Fix state if stuck
                return;
            }

            int rentAmount = currentSpace.CalculateRent();

            if (activePlayer.Money < rentAmount)
            {
                Console.WriteLine("jogador não tem dinheiro suficiente.");
                PlayerBankrupt(activePlayer);
            }

            activePlayer.Money -= rentAmount;
            currentSpace.Owner.Money += rentAmount;
            activePlayer.NeedsToPayRent = false;
            Console.WriteLine("Aluguer pago.");

        }

        public void BuyHouse(string[] parts)
        {
            if(parts.Length != 3)
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
            string houseSpaceName = parts[2];
            Player activePlayer = GetActivePlayer(playerName);

            if(activePlayer != activePlayersInGameWithOrder[currentPlayerIndex])
            {
                Console.WriteLine("Não é o vez do jogador.");
                return;
            }


            Space CurrentSpace = board.Grid[activePlayer.Row, activePlayer.Col];
            if (CurrentSpace.Name != houseSpaceName)
            {
                Console.WriteLine("Não é possível comprar casa no espaço indicado.");
                return;
            }

            if (CurrentSpace.Type != SpaceType.Street)
            {
                Console.WriteLine("Não é possível comprar casa no espaço indicado."); 
                return;
            }
             
             //Checks if Player owns all properties in the color group
            if(!OwnsAllColor(activePlayer, CurrentSpace.ColorGroup))
            {
                Console.WriteLine("Não é possível comprar casa no espaço indicado.");
                return;
            }
            
            if(CurrentSpace.HouseCount >= 4)
            {
                Console.WriteLine("Número máximo de casas atingido.");
                return;
            }

            int houseCost = CurrentSpace.HouseCost();
            if(activePlayer.Money < houseCost)
            {
                Console.WriteLine("jogador não tem dinheiro suficiente.");
                return;
            }

            activePlayer.Money -= houseCost;
            CurrentSpace.HouseCount += 1;
            Console.WriteLine("Casa adquirida.");

            

        }

        public void TakeCard(string[] parts)
        {
            string playerName = parts[1];
            Player activePlayer = GetActivePlayer(playerName);
            Space space = board.Grid[activePlayer.Row, activePlayer.Col];

            if(parts.Length != 2)
            {
                Console.WriteLine("Instrução inválida.");
                return;
            }
            if (!gameInProgress)
            {
                Console.WriteLine("Não existe jogo em curso.");
                return;
            }

            if(activePlayer == null)
            {
                Console.WriteLine("Jogador não participa no jogo em curso.");
                return;
            }

            if(activePlayer != activePlayersInGameWithOrder[currentPlayerIndex])
            {
                Console.WriteLine("Não é o vez do jogador.");
                return;
            }

            if(!activePlayer.HasCommunityOrChanceCard)
            {
                Console.WriteLine("O jogador não necessita de tirar carta.");
                return;
            }

            if (!activePlayer.NeedsToPayRent)
            {
                Console.WriteLine("A Carta já foi tirada.");
            }

            if(space.Type != SpaceType.Chance && space.Type != SpaceType.Community)
            {
                Console.WriteLine("Não é possível tirar uma carta neste espaço.");
                return;
            }

            int roll = random.Next(1, 101);
            

            if(space.Type != SpaceType.Chance)
            {
                if(roll <= 20)
                {
                    Console.WriteLine("O jogador recebeu 150.");
                    activePlayer.Money += 150;
                }else if(roll <= 30)
                {
                    Console.WriteLine("O jogador recebeu 200.");
                    activePlayer.Money += 200;
                }else if(roll <= 40)
                {
                    Console.WriteLine("O jogador tem de pagar 70.");
                    if(activePlayer.Money < 70)
                    {
                        PlayerBankrupt(activePlayer);
                    }
                    else
                    {
                        activePlayer.Money -= 70;
                        freeParkingFunds += 70;
                    }
                }else if(roll <= 60)
                {
                    Console.WriteLine("O jogador move-se para a casa Start.");
                    activePlayer.Row = 4;
                    activePlayer.Col = 4;
                    activePlayer.Money += 200;
                }else if(roll <= 80)
                {
                    Console.WriteLine("O jogador move-se para a casa Police.");
                    GoToJail(activePlayer);
                }else if(roll <= 100)
                {
                    Console.WriteLine("O jogador move-se para a casa FreePark.");
                    activePlayer.Row = 7;
                    activePlayer.Col = 1;
                    activePlayer.Money += freeParkingFunds;
                    
                }
            }
            else //SpaceType.Community
            {
                if(roll <= 10)
                {
                    Console.WriteLine("O jogador paga 20 por cada casa nos seus espaços.");
                    int totalHouses = 0;
                    foreach(var boardSpace in board.Grid)
                    {
                        if(boardSpace.Owner == activePlayer)
                        {
                            totalHouses += boardSpace.HouseCount;
                        }
                    }
                    int cost = totalHouses * 20;
                    if(activePlayer.Money < cost)
                    {
                        PlayerBankrupt(activePlayer);
                    }else
                    {
                        activePlayer.Money -= cost;
                    }
                }else if(roll <= 20)
                {
                    Console.WriteLine("O jogador recebe 10 por cada outro jogador.");
                    int CollectedAmount = 0;
                    foreach(var otherPlayer in activePlayersInGameWithOrder)
                    {
                        if(otherPlayer == activePlayer)
                        {
                            continue;
                        }
                        if(otherPlayer.Money >= 10)
                        {
                            otherPlayer.Money -= 10;
                            CollectedAmount += 10;
                        }
                    }
                    
                    activePlayer.Money += CollectedAmount;
                }else if(roll <= 40)
                {
                    Console.WriteLine("O jogador recebe 200.");
                    activePlayer.Money += 200;
                }else if(roll <= 60)
                {
                    Console.WriteLine("O jogador recebe 170.");
                    activePlayer.Money += 170;
                }else if(roll <= 70)
                {
                    Console.WriteLine("O jogador tem de pagar 40.");
                    activePlayer.Money -= 40;
                }else if(roll <= 80)
                {
                    Console.WriteLine("O jogador move-se para Pink1.");
                    activePlayer.Row = 6;
                    activePlayer.Col = 1;
                }else if(roll <= 90)
                {
                    Console.WriteLine("O jogador move-se para Teal2.");
                    activePlayer.Row = 4;
                    activePlayer.Col = 5;
                }else if(roll <= 100)
                {
                    Console.WriteLine("O jogador move-se para White2.");
                    activePlayer.Row = 2;
                    activePlayer.Col = 7;
                }
            }


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

        public bool OwnsAllColor(Player player, ColorType colorGroup)
        {
            foreach (var space in board.Grid)
            {
                if (space.ColorGroup == colorGroup)
                {
                    if (space.Owner != player)
                    {
                        return false;
                    }
                }
            }
            return true;
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