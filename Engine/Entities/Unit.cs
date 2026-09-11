// Engine/Entities/Unit.cs
namespace OgreBattle.Engine.Entities
{
    public class Unit
    {
        public string Id { get; set; }
        public string ClassId { get; set; }

        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int STR { get; set; }
        public int AGI { get; set; }
        public int INT { get; set; }

        public bool IsAlive => HP > 0;

        public Unit(string id, string classId)
        {
            Id = id;
            ClassId = classId;
        }
    }
}
