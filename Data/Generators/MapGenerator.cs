using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;

namespace HexGeneral.Game.Generators;

public static class MapGenerator
{
    public static Map Generate(HexGeneralData data, NewGameSettings settings)
    {
        var map = Map.Create(data);

        var h = (int)settings.Height.Value;
        var w = (int)settings.Width.Value;
        
        for (var i = 0; i < h; i++)
        {
            for (var j = 0; j < w; j++)
            {
                var gridCoord = new Vector2I(j, i);
                var coord = gridCoord.GridCoordsToCube();
                var hex = new Hex(coord);
                map.Hexes.Add(coord, hex);
            }
        }

        return map;
    }
}