// Engine/World/City.cs
namespace OgreBattle.Engine.World
{
    public class City
    {
        public string Name { get; set; }
        public int OwnerId { get; set; }

        public City(string name, int ownerId)
        {
            Name = name;
            OwnerId = ownerId;
        }
    }
}
