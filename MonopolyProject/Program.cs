using System;
using MonopolyProject.Logic;

namespace MonopolyProject
{
    class Program
    {
        static void Main(string[] args)
        {
            string readInput;
            GameEngine gameEngine = new GameEngine();

            while (true)
            {               
                readInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(readInput))
                {
                    break;
                }
                gameEngine.commandReading(readInput);
            }
        }
    }
}