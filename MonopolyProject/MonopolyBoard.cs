// Nome do Ficheiro: MonopolyBoard.cs
using System;
using Resisto_dos_jogadores; // Referência aos Jogadores
using System.Collections.Generic;
using System.Linq;

namespace MonopolyBoard
{
    public class Board
    {
        private readonly string[,] spaces;
        private const int Size = 7;
        private const int CellWidth = 13; 
        private const int CentroTabuleiro = 3; // Offset para traduzir coordenadas

        public Board()
        {
            spaces = new string[Size, Size]
            {
                // Col 0                 Col 1                Col 2                  Col 3                Col 4                Col 5                Col 6
                { "Prison",            "Green3",            "Violet1",             "Train2",            "Red3",              "White1",            "BackToStart" },  // Row 0
                { "Blue3",             "Community",         "Red2",                "Violet2",           "Water Works",       "Chance",            "White2" },       // Row 1
                { "Blue2",             "Red1",              "Chance",              "Brown2",            "Community",         "Black1",            "Lux Tax" },      // Row 2
                { "Train1",            "Green2",            "Teal1",               "Start",             "Teal2",             "Black2",            "Train3" },       // Row 3
                { "Blue1",             "Green1",            "Community",           "Brown1",            "Chance",            "Black3",            "White3" },       // Row 4
                { "Pink1",             "Chance",            "Orange1",             "Orange2",           "Orange3",           "Community",         "Yellow3" },      // Row 5
                { "FreePark",          "Pink2",             "Electric Company",    "Train4",            "Yellow1",           "Yellow2",           "Police" }        // Row 6
            };
        }

        public void Display(IEnumerable<SistemaJogo.Jogador> players)
        {
            Console.WriteLine("\t\t\t\t    === Tabuleiro de Monopólio 7x7 ===\n"); 
            DrawTopBorder();

            for (int i = 0; i < Size; i++)
            {
                // Passa os jogadores para o método DrawRow
                DrawRow(i, players);
                if (i < Size - 1)
                    DrawMiddleBorder();
            }

            DrawBottomBorder();
        }
        
        public string GetSpaceName(int row, int col)
        {
            if (row >= 0 && row < Size && col >= 0 && col < Size)
            {
                return spaces[row, col];
            }
            return "Fora do Tabuleiro";
        }

        public Tuple<int, int> GetSpaceCoords(string name)
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (spaces[r, c].Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return new Tuple<int, int>(r, c); // Encontrado!
                    }
                }
            }
            
            Console.WriteLine($"AVISO: Casa '{name}' não encontrada no tabuleiro!");
            return new Tuple<int, int>(3, 3); // Posição (3, 3) = Start
        }


        // --- Métodos de Desenho (Atualizados) ---
        private void DrawTopBorder()
        {
            Console.Write("┌");
            for (int i = 0; i < Size; i++)
            {
                Console.Write(new string('─', CellWidth));
                if (i < Size - 1) Console.Write("┬");
            }
            Console.WriteLine("┐");
        }

        private void DrawMiddleBorder()
        {
            Console.Write("├");
            for (int i = 0; i < Size; i++)
            {
                Console.Write(new string('─', CellWidth));
                if (i < Size - 1) Console.Write("┼");
            }
            Console.WriteLine("┤");
        }

        private void DrawBottomBorder()
        {
            Console.Write("└");
            for (int i = 0; i < Size; i++)
            {
                Console.Write(new string('─', CellWidth));
                if (i < Size - 1) Console.Write("┴");
            }
            Console.WriteLine("┘");
        }

        // <-- MUDANÇA AQUI: DrawRow() agora desenha DUAS linhas por célula -->
        private void DrawRow(int row, IEnumerable<SistemaJogo.Jogador> players)
        {
            // --- Linha 1: Nome da Casa ---
            Console.Write("│");
            for (int j = 0; j < Size; j++) // j = coluna
            {
                string spaceName = spaces[row, j];
                string centeredName = CenterText(spaceName, CellWidth);
                Console.Write($"{centeredName}│");
            }
            Console.WriteLine(); // Fim da linha 1

            // --- Linha 2: Marcadores dos Jogadores ---
            Console.Write("│");
            for (int j = 0; j < Size; j++) // j = coluna
            {
                // 1. Encontrar jogadores que estão nesta casa
                var playersOnThisSpace = players
                    .Where(p => p.EstaEmJogo && 
                                (p.PosicaoY + CentroTabuleiro) == row && 
                                (p.PosicaoX + CentroTabuleiro) == j)
                    .ToList();

                string markerText = " "; // Default: um espaço vazio
                
                if (playersOnThisSpace.Any())
                {
                    // 2. Se há jogadores, criar a string de marcadores (iniciais)
                    string playerMarkers = "";
                    foreach (var p in playersOnThisSpace)
                    {
                        if (!string.IsNullOrEmpty(p.Nome))
                        {
                            playerMarkers += p.Nome[0]; // Adiciona a primeira letra
                        }
                    }
                    markerText = playerMarkers;
                }
                
                // 3. Centrar os marcadores
                string centeredMarkers = CenterText(markerText, CellWidth);
                Console.Write($"{centeredMarkers}│");
            }
            Console.WriteLine(); // Fim da linha 2
        }

        private string CenterText(string text, int width)
        {
            text = text.Trim();
            if (text.Length > width)
            {
                return text.Substring(0, width); // Corta se for demasiado longo
            }
            if (text.Length == width)
            {
                return text;
            }
            int padding = width - text.Length;
            int padLeft = padding / 2;
            int padRight = padding - padLeft;
            return new string(' ', padLeft) + text + new string(' ', padRight);
        }
    }
}