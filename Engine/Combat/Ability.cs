// Engine/Combat/Ability.cs
namespace OgreBattle.Engine.Combat
{
    public class Ability
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Power { get; set; }
        public bool IsMagic { get; set; }

        public Ability(string id, string name, int power, bool isMagic = false)
        {
            Id = id;
            Name = name;
            Power = power;
            IsMagic = isMagic;
        }
    }
}
