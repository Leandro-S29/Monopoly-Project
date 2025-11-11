using System;

namespace MonopolyBoard
{
    public class Board
    {
        private readonly string[,] spaces;
        private const int Size = 7;
        
        // Largura da célula reduzida
        private const int CellWidth = 13; 

        public Board()
        {
            // Array formatado para ser legível
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

        public void Display()
        {
            Console.WriteLine("\t\t\t\t    === Tabuleiro de Monopólio 7x7 ===\n"); // Título ajustado
            DrawTopBorder();

            for (int i = 0; i < Size; i++)
            {
                DrawRow(i);
                if (i < Size - 1)
                    DrawMiddleBorder();
            }

            DrawBottomBorder();
        }
        
        // Método para obter o nome da casa
        public string GetSpaceName(int row, int col)
        {
            if (row >= 0 && row < Size && col >= 0 && col < Size)
            {
                return spaces[row, col];
            }
            return "Fora do Tabuleiro";
        }


        // --- Métodos de Desenho ---
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

        private void DrawRow(int row)
        {
            Console.Write("│");
            for (int j = 0; j < Size; j++)
            {
                string text = CenterText(spaces[row, j], CellWidth);
                Console.Write($"{text}│");
            }
            Console.WriteLine();
        }

        // Método 'CenterText' atualizado para truncar nomes compridos
        private string CenterText(string text, int width)
        {
            text = text.Trim();

            // Se o texto for maior que a célula, encurta-o
            if (text.Length > width)
            {
                // Retorna os primeiros 'width-1' caracteres + "."
                return text.Substring(0, width - 1) + "."; 
            }
            
            if (text.Length == width)
            {
                return text;
            }

            // Lógica original para centrar
            int padding = width - text.Length;
            int padLeft = padding / 2;
            int padRight = padding - padLeft;
            return new string(' ', padLeft) + text + new string(' ', padRight);
        }
    }
}