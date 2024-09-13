using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;

namespace HexGeneral.Game;

public class MapUnitHolder(int id, 
    Dictionary<ERef<Unit>, HexRef> unitPositions, 
    Dictionary<HexRef, List<ERef<Unit>>> hexLandUnits) : Entity(id)
{
    public static int MaxLandUnitsPerHex { get; private set; } = 3;
    public Dictionary<ERef<Unit>, HexRef> UnitPositions { get; private set; } = unitPositions;
    public Dictionary<HexRef, List<ERef<Unit>>> HexLandUnits { get; private set; } = hexLandUnits;

    public void DeployUnit(Unit u, Hex hex)
    {
        if (unitPositions.ContainsKey(u.MakeRef()))
        {
            throw new Exception();
        }
        UnitPositions.Add(u.MakeRef(), hex.MakeRef());
        HexLandUnits.AddOrUpdate(hex.MakeRef(), u.MakeRef());
    }

    public void SetUnitPosition(Unit u, HexRef pos, ProcedureKey key)
    {
        if (UnitPositions.TryGetValue(u.MakeRef(), out var prev))
        {
            HexLandUnits[prev].Remove(u.MakeRef());
        }

        HexLandUnits.AddOrUpdate(pos, u.MakeRef());
        UnitPositions[u.MakeRef()] = pos;
    }
    public override void Made(Data d)
    {
        d.SetEntitySingleton<MapUnitHolder>();
    }

    public override void CleanUp(Data d)
    {
        throw new System.Exception();
    }

    public void Remove(Unit u)
    {
        var r = u.MakeRef();
        if (UnitPositions.Remove(r, out var coords))
        {
            HexLandUnits[coords].Remove(r);
        }
    }
}