using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;
using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Game;

public class Hex(Vector3I coords, ModelIdRef<Landform> landform, 
    ModelIdRef<Vegetation> vegetation, int id, 
    ERef<Regime> regime)
    : IIded
{
    public int Id { get; private set; } = id;
    public Vector3I Coords { get; private set; } = coords;
    public ModelIdRef<Landform> Landform { get; private set; } = landform;
    public ModelIdRef<Vegetation> Vegetation { get; private set; } = vegetation;
    public ERef<Regime> Regime { get; private set; } = regime;
    public List<Color> DebugColors { get; set; } = new List<Color>();
    
    
    public void SetLandform(ModelIdRef<Landform> lf)
    {
        Landform = lf;
    }
    public void SetVegetation(ModelIdRef<Vegetation> v)
    {
        Vegetation = v;
    }

    public void SetRegime(ERef<Regime> regime)
    {
        Regime = regime;
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

    public Color GetTerrainColor(HexGeneralData data)
    {
        var worldPos = Coords.CubeToGridCoords().GetWorldPos();
        var hexTris = ShapeBuilder.GetHex(worldPos, 1f);

        var v = Vegetation.Get(data);

        Color color;
        if (v.Color == Colors.Transparent)
        {
            color = Landform.Get(data).Color;
        }
        else
        {
            color = v.Color.Darkened(Landform.Get(data).DarkenFactor);
        }

        // color = color.Darkened(data.Random.RandfRange(-HexBaseColorGraphics.ColorWobble,
        //     HexBaseColorGraphics.ColorWobble));
        return color;
    }
}