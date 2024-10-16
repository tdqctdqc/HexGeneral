using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
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
    
    
    public void SetLandformGen(ModelIdRef<Landform> lf)
    {
        Landform = lf;
    }

    public void SetLandform(Landform lf, ProcedureKey key)
    {
        Landform = lf.MakeIdRef(key.Data);
        key.Data.Data().Notices.HexAltered?.Invoke(this);
    }
    
    public void SetVegetationGen(ModelIdRef<Vegetation> v)
    {
        Vegetation = v;
    }
    public void SetVegetation(Vegetation v, ProcedureKey key)
    {
        Vegetation = v.MakeIdRef(key.Data);
        key.Data.Data().Notices.HexAltered?.Invoke(this);
    }
    public void SetRegimeGen(ERef<Regime> regime)
    {
        Regime = regime;
    }
    public void SetRegime(ERef<Regime> regime, ProcedureKey key)
    {
        Regime = regime;
        key.Data.Data().Notices.HexAltered.Invoke(this);
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

    public bool TryGetLocation(HexGeneralData data, out Location loc)
    {
        loc = null;
        if (data.LocationHolder.Locations.TryGetValue(MakeRef(),
                out var l))
        {
            loc = l.Get(data);
            return true;
        }
        return false;
    }
    
    public bool Full(Domain domain, HexGeneralData data)
    {
        return data.MapUnitHolder
                   .HexUnitsByDomain[domain.MakeIdRef(data)]
                   .TryGetValue(MakeRef(), out var units)
                        && units.Count == MapUnitHolder.MaxUnitsPerHex;
    }

    public bool CanDeploy(Domain domain, HexGeneralData data)
    {
        if (Full(domain, data)) return false;
        if(
        TryGetLocation(data, out var loc) == false
            || loc.Buildings
                .Any(b => b.Get(data) is ISupplyCenter)
            == false)
        {
            return false;
        }

        return true;
    }
    public IEnumerable<Unit> GetAllUnits(HexGeneralData data)
    {
        var hRef = MakeRef();
        return data.MapUnitHolder
            .HexUnitsByDomain
            .Where(kvp => kvp.Value.ContainsKey(hRef))
            .SelectMany(kvp => kvp.Value[hRef])
            .Select(u => u.Get(data));
    }
    public IEnumerable<Unit> GetDomainUnits(
        Domain domain, HexGeneralData data)
    {
        return data.MapUnitHolder.HexUnitsByDomain[domain.MakeIdRef(data)]
            .TryGetValue(MakeRef(), out var units)
            ? units.Select(u => u.Get(data))
            : ImmutableArray<Unit>.Empty;
    }

    public HexRef MakeRef()
    {
        return new HexRef(Coords);
    }
}