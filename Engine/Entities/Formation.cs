// Engine/Entities/Formation.cs
using System.Collections.Generic;

namespace OgreBattle.Engine.Entities
{
    public class Formation
    {
        public List<Unit> FrontRow { get; } = new List<Unit>();
        public List<Unit> MiddleRow { get; } = new List<Unit>();
        public List<Unit> BackRow { get; } = new List<Unit>();

        public void AssignToRow(Unit unit, int rowIndex)
        {
            switch (rowIndex)
            {
                case 0: FrontRow.Add(unit); break;
                case 1: MiddleRow.Add(unit); break;
                case 2: BackRow.Add(unit); break;
            }
        }
    }
}
