using System;

// Namespace para organizar a lógica do jogo
namespace MonopolyGameLogic
{
    // 'struct' para guardar o resultado dos dois dados
    public readonly struct DiceResult
    {
        public int HorizontalMove { get; }
        public int VerticalMove { get; }
        
        // <-- MUDANÇA 1: Nova propriedade para sabermos se os valores são iguais -->
        public bool IsDoubles { get; }

        public DiceResult(int horizontal, int vertical, bool isDoubles)
        {
            HorizontalMove = horizontal;
            VerticalMove = vertical;
            // <-- MUDANÇA 2: Atribuir a nova propriedade -->
            IsDoubles = isDoubles;
        }
    }

    public class DiceRoller
    {
        private readonly Random random;

        // A lista de valores possíveis para um dado, excluindo o zero
        private readonly int[] possibleValues = { -3, -2, -1, 1, 2, 3 };

        public DiceRoller()
        {
            random = new Random();
        }

        // Método privado que lança UM dado
        private int RollSingleDie()
        {
            // Escolhe um índice aleatório (0 a 5)
            int index = random.Next(0, possibleValues.Length);
            return possibleValues[index];
        }

        // Método público que o SistemaJogo vai chamar
        public DiceResult Roll()
        {
            int horizontal = RollSingleDie();
            int vertical = RollSingleDie();
            
            // <-- MUDANÇA 3: Verificar se são iguais e passar essa informação -->
            bool doubles = (horizontal == vertical);
            
            return new DiceResult(horizontal, vertical, doubles);
        }
    }
}