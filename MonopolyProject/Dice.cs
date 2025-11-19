namespace MonopolyProject
{
    public static class Dice
    {
        private static Random random = new Random();

        // Rolls the dice to get a value between -3 and 3, skipping zero
        public static int Roll()
        {
            while (true)
            {
                int roll = random.Next(-3, 4); // Generates numbers from -3 to 3
                if (roll != 0)
                {
                    return roll;
                }
            }
        }
    }
}