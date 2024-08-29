using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class Hex(Vector3I coords, ModelIdRef<Landform> landform, ModelIdRef<Vegetation> vegetation)
{
    
    public Vector3I Coords { get; private set; } = coords;
    public ModelIdRef<Landform> Landform { get; private set; } = landform;
    public ModelIdRef<Vegetation> Vegetation { get; private set; } = vegetation;
    public Color Chunk1Color { get; set; }
    public void SetLandform(ModelIdRef<Landform> lf)
    {
        Landform = lf;
    }
    public void SetVegetation(ModelIdRef<Vegetation> v)
    {
        Vegetation = v;
    }

    public IEnumerable<Hex> GetNeighbors(HexGeneralData data)
    {
        var map = data.Map;
        return HexExt.HexDirs.Where(dir => map.Hexes.ContainsKey(dir + Coords))
            .Select(dir => map.Hexes[dir + Coords]);
    }
    public static IEnumerable<Vector3I> GetNeighborCoords(
        Vector3I coords, HexGeneralData data)
    {
        var map = data.Map;
        for (var i = 0; i < HexExt.HexDirs.Count; i++)
        {
            var dir = HexExt.HexDirs[i];
            if (map.Hexes.ContainsKey(coords + dir))
            {
                yield return coords + dir;
            }
        }
    }

    public Vector2 WorldPos()
    {
        return Coords.CubeToGridCoords().GetWorldPos();
    }
}