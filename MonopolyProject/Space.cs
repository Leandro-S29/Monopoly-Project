namespace MonopolyProject
{
    public class Space
    {
        public string Name { get; }
        public SpaceType Type { get; }
        public ColorGroup Color { get; }
        public int Price { get; }
        public Player? Owner { get; set; }
        public int HouseCount { get; set; }

        public Space(string name, SpaceType type, ColorGroup color, int price)
        {
            Name = name;
            Type = type;
            Color = color;
            Price = price;
            Owner = null;
            HouseCount = 0;
        }

        // Calculates rent based on property type and number of houses
        public int GetRent()
        {
            if (Type == SpaceType.Property)
            {
                // Formula: Base 25% + 75% per house
                return (int)(Price * 0.25 + Price * 0.75 * HouseCount);
            }
            // Fallback for Trains/Utilities (25% flat rate)
            return (int)(Price * 0.25);
        }

        // Calculates the cost to purchase a house on this space
        public int GetHousePrice()
        {
            // 60% of the property price
            return (int)(Price * 0.6);
        }
    }
}