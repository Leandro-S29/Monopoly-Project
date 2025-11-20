namespace MonopolyProject.Models
{
    public class Space
        {
            public string Name { get; set; }
            public SpaceType Type { get; set; }
            public int Price { get; set; }
            public ColorType ColorGroup { get; set; } 
            public Player Owner { get; set; }
            public int HouseCount { get; set; }

            public Space(string name, SpaceType type, int price, ColorType colorGroup)
            {
                Name = name;
                Type = type;
                Price = price;
                ColorGroup = colorGroup;
                Owner = null;
                HouseCount = 0;
            }

            // Method to calculate rent based on house count
            public int CalculateRent()
            {
                double baseRent = Price * 0.25;
                double houseRent = Price * 0.75 * HouseCount;
                return (int)(baseRent + houseRent);
            }

            // Method to get the cost of building a house
            public int HouseCost()
            {
                return (int)(Price * 0.6);
            }
        }
}