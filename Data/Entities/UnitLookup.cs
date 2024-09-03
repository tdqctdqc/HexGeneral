using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class UnitLookup(int id, Dictionary<ERef<Unit>, Vector3I> unitPositions, Dictionary<Vector3I, List<ERef<Unit>>> hexLandUnits) : Entity(id)
{
    public Dictionary<ERef<Unit>, Vector3I> UnitPositions { get; private set; } = unitPositions;
    public Dictionary<Vector3I, List<ERef<Unit>>> HexLandUnits { get; private set; } = hexLandUnits;

    
    public override void Made(Data d)
    {
        
    }

    public override void CleanUp(Data d)
    {
        throw new System.Exception();
    }
}