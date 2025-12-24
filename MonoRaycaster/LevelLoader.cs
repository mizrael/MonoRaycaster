using System;
using System.Collections.Generic;

namespace MonoRaycaster;

public class LevelLoader
{
    public static Map LoadFromJson(string path)
    {
        var json = System.IO.File.ReadAllText(path);
        var levelData = System.Text.Json.JsonSerializer.Deserialize<LevelData>(json);
        if (levelData == null)
            throw new Exception("Failed to deserialize map data.");
        var map = new Map(levelData.Map.Cells);
        return map;
    }

    private class LevelData
    {
        public required MapData Map { get; set; }
        public required List<EntityData> Entities { get; set; }
    }

    private class MapData
    {
        public required int[][] Cells { get; set; }
    }

    private class EntityData
    {
        public required string Type { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
}