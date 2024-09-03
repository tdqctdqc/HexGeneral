using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class MapUnitHolder(int id, Dictionary<ERef<Unit>, Vector3I> unitPositions, Dictionary<Vector3I, List<ERef<Unit>>> hexLandUnits) : Entity(id)
{
    public Dictionary<ERef<Unit>, Vector3I> UnitPositions { get; private set; } = unitPositions;
    public Dictionary<Vector3I, List<ERef<Unit>>> HexLandUnits { get; private set; } = hexLandUnits;

    public void DeployUnit(Unit u, Hex hex)
    {
        if (unitPositions.ContainsKey(u.MakeRef()))
        {
            throw new Exception();
        }
        UnitPositions.Add(u.MakeRef(), hex.Coords);
        HexLandUnits.AddOrUpdate(hex.Coords, u.MakeRef());
    }
    public override void Made(Data d)
    {
        d.SetEntitySingleton<MapUnitHolder>();
    }

    public override void CleanUp(Data d)
    {
        throw new System.Exception();
    }
}