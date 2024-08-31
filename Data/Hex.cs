using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class Hex(Vector3I coords, ModelIdRef<Landform> landform, ModelIdRef<Vegetation> vegetation, int id)
    : IIded
{
    public int Id { get; private set; } = id;
    public Vector3I Coords { get; private set; } = coords;
    public ModelIdRef<Landform> Landform { get; private set; } = landform;
    public ModelIdRef<Vegetation> Vegetation { get; private set; } = vegetation;
    public List<Color> Colors { get; set; } = new List<Color>();
    public void SetLandform(ModelIdRef<Landform> lf)
    {
        Landform = lf;
    }
    public void SetVegetation(ModelIdRef<Vegetation> v)
    {
        Vegetation = v;
    }

    private Hex[] _neighbors;
    public IEnumerable<Hex> GetNeighbors(HexGeneralData data)
    {
        var map = data.Map;
        if (_neighbors is null)
        {
            _neighbors = HexExt.HexDirs.Where(dir => map.Hexes.ContainsKey(dir + Coords))
                .Select(dir => map.Hexes[dir + Coords])
                .ToArray();
        }
        return _neighbors;
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

    private Vector2I _gridCoords = Vector2I.MaxValue;
    public Vector2I GetGridCoords()
    {
        if (_gridCoords == Vector2I.MaxValue)
        {
            _gridCoords = Coords.CubeToGridCoords();
        }

        return _gridCoords;
    }
}