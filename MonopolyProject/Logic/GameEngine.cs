using System;
using System.Collections.Generic;
using System.Data;
using MonopolyProject.Models;

namespace MonopolyProject.Logic
{
    public class GameEngine
    {
        private bool gameInProgress;
        private int freeParkingFunds;


        private Dictionary<string, Player> registeredPlayers = new Dictionary<string, Player>();
        private Dictionary<string, Player> activeInGamePlayers = new Dictionary<string, Player>();



        public GameEngine()
        {
            registeredPlayers = new Dictionary<string, Player>();
            activeInGamePlayers = new Dictionary<string, Player>();
            gameInProgress = false;
        }


        // Method to read and process commands
        public void commandReading(string command)
        {
            if(string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            string[] commandParts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if(commandParts.Length == 0)
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
                    //TODO: Start Game
                    break;
                case "LD":
                    //TODO: Roll Dice's
                    break;
                case "CE":
                    //TODO: Buy space
                    break;
                case "DJ":
                    //TODO: Game Details
                    break;
                case "TT":
                    //TODO: End Turn
                    break;
                case "PA":
                    //TODO: Pay Rent
                    break;
                case "CC":
                    //TODO: Buy House
                    break;
                case "TC":
                    //TODO: Take Cards
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
            else
            {
                registeredPlayers.Add(name, new Player(name));
                Console.WriteLine("Jogador registrado com sucesso.");
            }
        }
        
        // Method to list all registered players
        public void ListPlayers()
        {
            if (registeredPlayers.Count == 0)
            {
                Console.WriteLine("Sem jogadores registados. ");
                return;
            }

            foreach (var playerEntry in registeredPlayers)
            {
                Player player = playerEntry.Value;
                Console.WriteLine($"{player.Name} {player.GamesPlayed} {player.Wins} {player.Draws} {player.Losses}");
            }
        }

        public void StartGame(string[] commandParts)
        {
            if(commandParts.Length != 4)
            {
                    Console.WriteLine("Instrução inválida.");
                    return;
            }
            else if(gameInProgress == true)
            {
                Console.WriteLine("Existe um jogo em curso.");
                return;
            }
        }

        //TODO: GoToJail Method
        public void GoToJail(Player player)
        {
            // Moves player to prison
            player.Row = 0; // Makes player go to row = 0
            player.Col = 0; // Makes player go to col = 0
            
            player.IsInJail = true;
            player.TurnsInJail = 0; // Resets turns in jail counter
            player.HasRolledThisTurn = true; // Locks player from rolling dice this turn
        }
        //TODO: GetActivePlayer Method
        public Player GetActivePlayer(String playerName)
        {
            if(activeInGamePlayers.ContainsKey(playerName))
            {
                return activeInGamePlayers[playerName];
            }
            return null;
        }
        //TODO: PlayerBankrupt Method
        public void PlayerBankrupt(Player player)
        {
            //Remove Player from the game 
            activeInGamePlayers.Remove(player.Name);
            
            //update the player's stats 
            player.Losses += 1;
            if(activeInGamePlayers.Count == 1)
            {
                gameInProgress = false;
                activeInGamePlayers.Values.First().Wins += 1;
            }
        }

    }
}