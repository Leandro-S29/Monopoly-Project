using MonopolyProject;

public class Program
{
    public static void Main(string[] args)
    {
        Game game = new Game();

        // Main application loop. Exits on empty input
        while (true)
        {
            string? line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                break; // Stop the program
            }

            // Split input line into arguments
            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                continue;
            }

            string command = parts[0];
            string output = "";

            try
            {
                switch (command)
                {
                    // Register Player
                    case "RJ":
                        if (parts.Length != 2) output = "Instrução inválida.";
                        else output = game.RegisterPlayer(parts[1]);
                        break;

                    // List Players
                    case "LJ":
                        if (parts.Length != 1) output = "Instrução inválida.";
                        else output = game.ListPlayers();
                        break;

                    // Start Game
                    case "IJ":
                        if (parts.Length != 5) output = "Instrução inválida.";
                        else output = game.StartGame(parts.Skip(1).ToArray());
                        break;

                    // Roll Dice
                    case "LD":
                        if (parts.Length != 2) output = "Instrução inválida.";
                        else output = game.RollDice(parts[1]);
                        break;

                    // Buy Property
                    case "CE":
                        if (parts.Length != 2) output = "Instrução inválida.";
                        else output = game.BuyProperty(parts[1]);
                        break;

                    // Game Details
                    case "DJ":
                        if (parts.Length != 1) output = "Instrução inválida.";
                        else output = game.GetGameDetails();
                        break;

                    // End Turn
                    case "TT":
                        if (parts.Length != 2) output = "Instrução inválida.";
                        else output = game.EndTurn(parts[1]);
                        break;

                    // Pay Rent
                    case "PA":
                        if (parts.Length != 2) output = "Instrução inválida.";
                        else output = game.PayRent(parts[1]);
                        break;

                    // Buy House
                    case "CC":
                        if (parts.Length != 3) output = "Instrução inválida.";
                        else output = game.BuyHouse(parts[1], parts[2]);
                        break;

                    // Draw Card
                    case "TC":
                        if (parts.Length != 2) output = "Instrução inválida.";
                        else output = game.DrawCard(parts[1]);
                        break;

                    // Invalid command
                    default:
                        output = "Instrução inválida.";
                        break;
                }
            }
            catch (Exception ex)
            {
                // Debug output for unexpected errors
                output = $"ERRO INTERNO: {ex.Message}";
            }

            // Display result
            Console.WriteLine(output);
        }
    }
}