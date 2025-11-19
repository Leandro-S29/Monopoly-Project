using System.Text;

namespace MonopolyProject
{
    // Tracks the current phase of a player's turn
    public enum TurnState
    {
        AwaitingRoll,   // Needs to roll dice
        AwaitingRent,   // Needs to pay rent
        AwaitingCard,   // Needs to draw a card
        AwaitingActions // Can buy, build, or end turn
    }

    public class Game
    {
        // List of all registered players
        private List<Player> allRegisteredPlayers = new List<Player>();
        
        // Players currently in an active game
        private List<Player> currentGamePlayers = new List<Player>();
        private Board gameBoard; 
        private int currentPlayerIndex = 0;
        private bool isGameInProgress = false;
        private TurnState currentTurnState = TurnState.AwaitingRoll;
        private bool hasPlayerRolledThisTurn = false;
        private Random cardRandom = new Random();

        public Game()
        {
            gameBoard = new Board();
        }

        private Player? GetCurrentPlayer()
        {
            if (!isGameInProgress || !currentGamePlayers.Any())
            {
                return null;
            }
            if (currentPlayerIndex >= currentGamePlayers.Count)
            {
                currentPlayerIndex = 0;
            }
            return currentGamePlayers[currentPlayerIndex];
        }

        // Sends a player directly to the Prison space [0,0]
        private void GoToJail(Player player)
        {
            player.SetPosition(0, 0); // Prison coordinates
            player.IsInJail = true;
            player.TurnsInJail = 0;
            player.DoublesInARow = 0;
            currentTurnState = TurnState.AwaitingActions; 
        }

        // ### Game Over Logic (Silent) ###

        // Handles player bankruptcy without console output
        private void HandleBankruptcy(Player player)
        {
            player.IsBankrupt = true;
            player.Losses++;
            player.GamesPlayed++;
            
            // Reset owned properties
            foreach (var space in player.Properties)
            {
                space.Owner = null;
                space.HouseCount = 0;
            }
            player.Properties.Clear();

            int removedPlayerIndex = currentGamePlayers.IndexOf(player);
            currentGamePlayers.Remove(player);

            // Adjust index to maintain turn order
            if (currentPlayerIndex > removedPlayerIndex)
            {
                currentPlayerIndex--;
            }
            else if (currentPlayerIndex == removedPlayerIndex)
            {
                if (currentPlayerIndex >= currentGamePlayers.Count)
                {
                    currentPlayerIndex = 0;
                }
                hasPlayerRolledThisTurn = false;
                currentTurnState = TurnState.AwaitingRoll;
            }

            // Check if the game has a winner
            CheckForGameEnd();
        }

        // Checks if only one player remains (Silent)
        private void CheckForGameEnd()
        {
            if (isGameInProgress && currentGamePlayers.Count == 1)
            {
                Player winner = currentGamePlayers[0];
                winner.Wins++;
                winner.GamesPlayed++;
                
                // Reset game state
                isGameInProgress = false;
                currentGamePlayers.Clear();
                currentPlayerIndex = 0;
            }
        }


        // ### Game Commands ###

        // Register a new player
        public string RegisterPlayer(string name)
        {
            if (allRegisteredPlayers.Any(p => p.Name == name))
            {
                return "Jogador existente.";
            }
            
            allRegisteredPlayers.Add(new Player(name));
            return "Jogador registado com sucesso.";
        }

        // List all players and statistics
        public string ListPlayers()
        {
            if (!allRegisteredPlayers.Any())
            {
                return "Sem jogadores registados.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("NomeJogador NumJogos NumVitórias NumEmpates NumDerrotas");

            var orderedPlayers = allRegisteredPlayers
                .OrderByDescending(p => p.Wins)
                .ThenBy(p => p.Name);

            foreach (var p in orderedPlayers)
            {
                sb.AppendLine($"{p.Name} {p.GamesPlayed} {p.Wins} {p.Draws} {p.Losses}");
            }
            return sb.ToString().TrimEnd();
        }

        // Start a new game with specific players
        public string StartGame(string[] playerNames)
        {
            if (isGameInProgress)
            {
                return "Existe um jogo em curso.";
            }

            foreach (var name in playerNames)
            {
                var player = allRegisteredPlayers.FirstOrDefault(p => p.Name == name);
                if (player == null)
                {
                    return "Jogador inexistente.";
                }
            }

            // Initialize game state
            gameBoard = new Board();
            currentGamePlayers.Clear();
            foreach (var name in playerNames)
            {
                var player = allRegisteredPlayers.First(p => p.Name == name);
                player.ResetForNewGame();
                currentGamePlayers.Add(player);
            }
            
            isGameInProgress = true;
            currentPlayerIndex = 0; 
            currentTurnState = TurnState.AwaitingRoll;
            hasPlayerRolledThisTurn = false;

            return "Jogo iniciado com sucesso.";
        }

        // Roll Dice command
        public string RollDice(string playerName)
        {
            // Validation checks
            if (!isGameInProgress)
            {
                return "Não existe um jogo em curso.";
            }
            if (!currentGamePlayers.Any(p => p.Name == playerName))
            {
                return "Jogador não participa no jogo em curso.";
            }
            var player = GetCurrentPlayer();
            if (player == null || player.Name != playerName)
            {
                return "Não é a vez do jogador.";
            }

            // Check logic state
            if (currentTurnState != TurnState.AwaitingRoll)
            {
                return "Não é a vez do jogador.";
            }

            hasPlayerRolledThisTurn = true; 
            int roll1 = Dice.Roll();
            int roll2 = Dice.Roll();
            bool isDoubles = roll1 == roll2;

            // Handle Jail logic
            if (player.IsInJail)
            {
                if (isDoubles)
                {
                    player.IsInJail = false;
                    player.TurnsInJail = 0;
                }
                else
                {
                    player.TurnsInJail++;
                    if (player.TurnsInJail >= 3)
                    {
                        player.IsInJail = false;
                        player.TurnsInJail = 0;
                    }
                    currentTurnState = TurnState.AwaitingActions; 
                    return $"Saiu {roll1}/{roll2} espaço Prison. Jogador só de passagem.";
                }
            }
            
            if (isDoubles)
            {
                player.DoublesInARow++;
                if (player.DoublesInARow == 2)
                {
                    GoToJail(player);
                    return $"Saiu {roll1}/{roll2} espaço Police. Jogador preso.";
                }
            }
            else
            {
                player.DoublesInARow = 0;
            }

            // Move player and determine landing effects
            Space landedOn = gameBoard.MovePlayer(player, roll1, roll2);
            string baseOutput = $"Saiu {roll1}/{roll2} espaço {landedOn.Name}.";
            string specialOutput = "";

            // Handle space specific actions
            switch (landedOn.Type)
            {
                case SpaceType.Property:
                case SpaceType.Train:
                case SpaceType.Utility:
                    if (landedOn.Owner == null)
                    {
                        specialOutput = "Espaço sem dono.";
                        currentTurnState = TurnState.AwaitingActions;
                    }
                    else if (landedOn.Owner == player)
                    {
                        specialOutput = "Espaço já comprada.";
                        currentTurnState = TurnState.AwaitingActions;
                    }
                    else
                    {
                        specialOutput = "Espaço já comprada por outro jogador. Necessário pagar renda.";
                        currentTurnState = TurnState.AwaitingRent; 
                    }
                    break;
                case SpaceType.Chance:
                case SpaceType.Community:
                    specialOutput = "Espaço especial. Tirar carta.";
                    currentTurnState = TurnState.AwaitingCard; 
                    break;
                case SpaceType.BackToStart:
                    player.SetPosition(3, 3);
                    specialOutput = "Peça colocada no espaço Start.";
                    currentTurnState = TurnState.AwaitingActions;
                    break;
                case SpaceType.Police:
                    GoToJail(player);
                    specialOutput = "Jogador preso.";
                    currentTurnState = TurnState.AwaitingActions;
                    break;
                case SpaceType.Prison:
                    specialOutput = "Jogador só de passagem.";
                    currentTurnState = TurnState.AwaitingActions;
                    break;
                case SpaceType.FreePark:
                    player.Money += gameBoard.FreeParkMoney;
                    specialOutput = $"Jogador recebe {gameBoard.FreeParkMoney}.";
                    gameBoard.FreeParkMoney = 0;
                    currentTurnState = TurnState.AwaitingActions;
                    break;
                case SpaceType.Start:
                    player.Money += 200;
                    currentTurnState = TurnState.AwaitingActions;
                    break;
                case SpaceType.LuxTax:
                    int tax = 80;
                    if (player.Money < tax)
                    {
                        HandleBankruptcy(player);
                    }
                    else
                    {
                        player.Money -= tax;
                        gameBoard.FreeParkMoney += tax;
                    }
                    currentTurnState = TurnState.AwaitingActions;
                    break;
            }

            if (!isDoubles || player.IsInJail)
            {
                // State already set
            }
            else
            {
                currentTurnState = TurnState.AwaitingRoll; // Roll again
                hasPlayerRolledThisTurn = false; 
            }

            return $"{baseOutput}{(string.IsNullOrEmpty(specialOutput) ? "" : " " + specialOutput)}";
        }

        // Buy Property command
        public string BuyProperty(string playerName)
        {
            var player = GetCurrentPlayer();
            if (player == null)
            {
                 return "Instrução inválida.";
            }

            Space space = gameBoard.GetSpace(player.PositionX, player.PositionY);

            // Error checks
            if (space.Owner != null)
            {
                return "O espaço já se encontra comprado.";
            }

            if (space.Type != SpaceType.Property && space.Type != SpaceType.Train && space.Type != SpaceType.Utility)
            {
                return "Este espaço não está para venda.";
            }

            if (player.Money < space.Price)
            {
                return "jogador não tem dinheiro suficiente para adquirir o espaço.";
            }
            
            // Only allow if player rolled dice (implied logic)
            if (!hasPlayerRolledThisTurn)
            {
                // No specific error message defined for this case
            }

            // Process purchase
            player.Money -= space.Price;
            space.Owner = player;
            player.Properties.Add(space);
            
            return "Espaço comprado.";
        }

        // Get Game Details command
        public string GetGameDetails()
        {
            if (!isGameInProgress)
            {
                return "Não existe jogo em curso.";
            }

            var sb = new StringBuilder();
            sb.Append(gameBoard.GetBoardDetails(currentGamePlayers));
            
            var player = GetCurrentPlayer();
            if (player != null)
            {
                sb.AppendLine(player.Name);
                sb.AppendLine(player.Money.ToString());
            }

            return sb.ToString();
        }

        // End Turn command
        public string EndTurn(string playerName)
        {
            var player = GetCurrentPlayer();
            
            // Validation
            if (player == null || player.Name != playerName)
            {
                return "Não é o turno do jogador indicado.";
            }
            
            if (!hasPlayerRolledThisTurn || 
                currentTurnState == TurnState.AwaitingRent || 
                currentTurnState == TurnState.AwaitingCard)
            {
                return "jogador ainda tem ações a fazer.";
            }

            if (!isGameInProgress)
            {
                return "Não existe jogo em curso."; 
            }
            
            // Move to next player
            currentPlayerIndex = (currentPlayerIndex + 1) % currentGamePlayers.Count;
            var nextPlayer = GetCurrentPlayer();

            // Reset turn state
            hasPlayerRolledThisTurn = false;
            currentTurnState = TurnState.AwaitingRoll;
            if (nextPlayer != null)
            {
                nextPlayer.DoublesInARow = 0; 
            }

            return $"Turno terminado. Novo turno do jogador {nextPlayer?.Name}.";
        }

        // Pay Rent command
        public string PayRent(string playerName)
        {
            var player = GetCurrentPlayer();
            Space? space = (player != null) ? gameBoard.GetSpace(player.PositionX, player.PositionY) : null;
            int rent = (space != null && space.Owner != null) ? space.GetRent() : 0;
            
            // Error checks
            
            if (currentTurnState != TurnState.AwaitingRent || space?.Owner == player)
            {
                return "Não é necessário pagar aluguer.";
            }

            if (player != null && player.Money < rent)
            {
                HandleBankruptcy(player);
                return "jogador não tem dinheiro suficiente.";
            }

            if (!isGameInProgress)
            {
                return "Não existe um jogo em curso.";
            }
            
            if (!currentGamePlayers.Any(p => p.Name == playerName))
            {
                return "Jogador não participa no jogo em curso.";
            }
            
            if (player == null || player.Name != playerName)
            {
                return "Não é a vez do jogador.";
            }

            // Process payment
            player.Money -= rent;
            if (space.Owner != null)
            {
                space.Owner.Money += rent;
            }

            currentTurnState = TurnState.AwaitingActions; 
            return "Aluguer pago.";
        }

        // Buy House command
        public string BuyHouse(string playerName, string spaceName)
        {
            var player = GetCurrentPlayer();
            var space = (player != null) ? player.Properties.FirstOrDefault(p => p.Name == spaceName) : null;

            // Error checks
            
            if (space == null || space.Type != SpaceType.Property || space.HouseCount >= 4)
            {
                return "Não é possível comprar casa no espaço indicado.";
            }
            
            var allSpacesInGroup = gameBoard.Where(s => s.Color == space.Color && s.Color != ColorGroup.None);
            bool ownsAll = allSpacesInGroup.All(s => s.Owner == player);
            if (!ownsAll)
            {
                return "0 jogador não possui todos os espaços da cor.";
            }

            int housePrice = space.GetHousePrice();
            if (player != null && player.Money < housePrice)
            {
                return "jogador não possui dinheiro suficiente.";
            }

            if (!isGameInProgress) 
            {
                return "Não existe um jogo em curso.";
            }

            if (player == null || !currentGamePlayers.Any(p => p.Name == playerName)) 
            {
                return "Jogador não participa no jogo em curso.";
            }
            
            if (player.Name != playerName) 
            {
                return "Não é a vez do jogador.";
            }

            // Process purchase
            player.Money -= housePrice;
            space.HouseCount++;
            return "Casa adquirida.";
        }
        
        // Draw Card command
        public string DrawCard(string playerName)
        {
            var player = GetCurrentPlayer();
            Space? space = (player != null) ? gameBoard.GetSpace(player.PositionX, player.PositionY) : null;

            // Error checks

            if (space == null || (space.Type != SpaceType.Chance && space.Type != SpaceType.Community))
            {
                return "Não é possível tirar carta neste espaço.";
            }
            
            if (currentTurnState != TurnState.AwaitingCard)
            {
                return "A carta já foi tirada.";
            }
            
            if (!isGameInProgress) 
            {
                return "Não existe um jogo em curso.";
            }

            if (player == null || !currentGamePlayers.Any(p => p.Name == playerName)) 
            {
                return "Jogador não participa no jogo em curso.";
            }

            if (player.Name != playerName) 
            {
                return "Não é a vez do jogador.";
            }

            string cardMessage = "";
            double chance = cardRandom.NextDouble(); 
            
            if (space.Type == SpaceType.Chance)
            {
                // Logic for Chance cards
                if (chance < 0.20)
                {
                    cardMessage = "O jogador recebe 150.";
                    player.Money += 150;
                }
                else if (chance < 0.30)
                {
                    cardMessage = "O jogador recebe 200.";
                    player.Money += 200;
                }
                else if (chance < 0.40)
                {
                    cardMessage = "O jogador tem de pagar 70.";
                    if (player.Money < 70) {
                        HandleBankruptcy(player);
                    } else {
                        player.Money -= 70; gameBoard.FreeParkMoney += 70;
                    }
                }
                else if (chance < 0.60)
                {
                    cardMessage = "O jogador move-se para a casa Start.";
                    player.SetPosition(3, 3);
                }
                else if (chance < 0.80)
                {
                    cardMessage = "O jogador move-se para a casa Police.";
                    GoToJail(player);
                }
                else
                {
                    cardMessage = "O jogador move-se para a casa FreePark.";
                    player.SetPosition(0, 6);
                }
            }
            else // Community Chest logic
            {
                if (chance < 0.10)
                {
                    int totalHouses = player.Properties.Sum(p => p.HouseCount);
                    int payment = 20 * totalHouses;
                    cardMessage = $"O jogador paga 20 por cada casa nos seus espaços. (Total: {payment})";
                    if (player.Money < payment) {
                        HandleBankruptcy(player);
                    } else {
                        player.Money -= payment; gameBoard.FreeParkMoney += payment;
                    }
                }
                else if (chance < 0.20)
                {
                    int received = 0;
                    foreach (var p in currentGamePlayers.Where(p => p != player))
                    {
                        if(p.Money >= 10) {
                            p.Money -= 10;
                            received += 10;
                        } else {
                            // Assumed: if opponent can't pay, nothing happens
                        }
                    }
                    player.Money += received;
                    cardMessage = $"O jogador recebe 10 de cada outro jogador. (Total: {received})";
                }
                else if (chance < 0.40)
                {
                    cardMessage = "O jogador recebe 100.";
                    player.Money += 100;
                }
                else if (chance < 0.60)
                {
                    cardMessage = "O jogador recebe 170.";
                    player.Money += 170;
                }
                else if (chance < 0.70)
                {
                    cardMessage = "O jogador tem de pagar 40.";
                    if (player.Money < 40) {
                        HandleBankruptcy(player);
                    } else {
                        player.Money -= 40; gameBoard.FreeParkMoney += 40;
                    }
                }
                else if (chance < 0.80)
                {
                    cardMessage = "O jogador move-se para Pink1.";
                    player.SetPosition(0, 5);
                }
                else if (chance < 0.90)
                {
                    cardMessage = "O jogador move-se para Teal2.";
                    player.SetPosition(4, 3);
                }
                else
                {
                    cardMessage = "O jogador move-se para White2.";
                    player.SetPosition(6, 1);
                }
            }
            
            currentTurnState = TurnState.AwaitingActions; 
            return cardMessage;
        }
    }
}