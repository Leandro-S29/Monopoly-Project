namespace MonopolyProject.Models
{
    public class Space
        {
            public string Name { get; set; }
            public SpaceType Type { get; set; }
            public double Price { get; set; }
            public ColorType ColorGroup { get; set; } 
            public Player Owner { get; set; }
            public int HouseCount { get; set; }

            public Space(string name, SpaceType type, double price, ColorType colorGroup)
            {
                Name = name;
                Type = type;
                Price = price;
                ColorGroup = colorGroup;
                Owner = null;
                HouseCount = 0;
            }

            // Method to calculate rent based on house count
            public double CalculateRent()
            {
                double baseRent = Price * 0.25;
                double houseRent = Price * 0.75 * HouseCount;
                return baseRent + houseRent;
            }

            // Method to get the cost of building a house
            public double HouseCost()
            {
                return Price * 0.6;
            }
        }
}