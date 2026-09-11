// Engine/Data/DataLoader.cs
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OgreBattle.Engine.Data
{
    public class UnitDefinition
    {
        public string Id { get; set; }
        public int HP { get; set; }
        public int STR { get; set; }
        public int AGI { get; set; }
        public int INT { get; set; }
        public List<string> Abilities { get; set; }
    }

    public class ClassDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
    }

    public class MapDefinition
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int[,] Terrain { get; set; }
    }

    public class DataLoader
    {
        public Dictionary<string, UnitDefinition> LoadUnitDefinitions(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, UnitDefinition>>(json);
        }

        public Dictionary<string, ClassDefinition> LoadClassDefinitions(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, ClassDefinition>>(json);
        }

        public MapDefinition LoadMapDefinition(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<MapDefinition>(json);
        }
    }
}
