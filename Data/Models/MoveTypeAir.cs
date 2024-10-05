using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game.Components;
using HexGeneral.Game.Logic;

namespace HexGeneral.Game;

public class MoveTypeAir : MoveType
{
    public override float GetMoveCost(Hex from, Hex to, Regime movingRegime, HexGeneralData data)
    {
        return 1f;
    }

    public override HashSet<Hex> GetMoveRadius(Hex start, 
        Regime movingRegime, float maxCost, HexGeneralData data)
    {
        return data.Map.Hexes.Values
            .Where(h => ValidDest(h, movingRegime, data))
            .ToHashSet();
    }

    public override void MoveThru(Hex hex, Unit unit, ProcedureKey key)
    {
        return;
    }


    public override void ModifyAsAttacker(CombatModifier modifier, HexGeneralData data)
    {
        //get damage from AA 
    }
    
    public override void ModifyAsDefender(CombatModifier modifier, HexGeneralData data)
    {
        
    }

    public override bool ValidDest(Hex hex, Regime movingRegime, HexGeneralData data)
    {
        var airDomain = data.ModelPredefs.Domains.AirDomain;
        
        if (hex.Regime != movingRegime)
        {
            return false;
        }

        if (hex.TryGetLocation(data, out var loc) == false
            || loc.Buildings.Any(b => b.Get(data) is AirbaseBuilding) == false)
        {
            return false;
        }
        
        if (hex.Full(airDomain, data))
        {
            return false;
        }

        

        return true;
    }
}