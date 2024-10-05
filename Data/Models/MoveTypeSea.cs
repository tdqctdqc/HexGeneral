using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.Logic;
using HexGeneral.Game.Logic;

namespace HexGeneral.Game;

public class MoveTypeSea : MoveType
{
    public override float GetMoveCost(Hex from, Hex to, Regime movingRegime, HexGeneralData data)
    {
        return 1f;
    }

    public override HashSet<Hex> GetMoveRadius(Hex start, 
        Regime movingRegime, float maxCost, HexGeneralData data)
    {
        var map = data.Map;
        return start.Coords.GetHexesInRadius(Mathf.FloorToInt(maxCost))
            .Where(c => map.Hexes.ContainsKey(c))
            .Select(c => map.Hexes[c])
            .Where(h => h.Landform.Get(data).IsLand == false)
            .ToHashSet();
    }

    public override void MoveThru(Hex hex, Unit unit, ProcedureKey key)
    {
        return;
    }

    public override void ModifyAsAttacker(CombatModifier modifier, HexGeneralData data)
    {
        
    }

    public override void ModifyAsDefender(CombatModifier modifier, HexGeneralData data)
    {
        
    }

    public override bool ValidDest(Hex hex, Regime movingRegime, HexGeneralData data)
    {
        return hex.Landform.Get(data).IsLand == false;
    }
}