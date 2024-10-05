using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.PathFinder;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game.Components;
using HexGeneral.Game.Logic;

namespace HexGeneral.Game;

public class MoveTypeLand : MoveType
{
    public static float HostileHexCostMult { get; private set; } = 1.5f;

    public Dictionary<Landform, float> LandformCosts { get; private set; }
    public Dictionary<Vegetation, float> VegetationCosts { get; private set; }
    public Dictionary<RoadModel, float> RoadCosts { get; private set; }
    public TerrainDamageModel AttackTerrainDamageModel { get; private set; }
    public TerrainDamageModel DefendTerrainDamageModel { get; private set; }
    
    public override float GetMoveCost(Hex from, Hex to, 
        Regime movingRegime, HexGeneralData data)
    {
        var mult = 1f;
        var lfCost = LandformCosts[to.Landform.Get(data)];
        if(lfCost < 0) return Single.PositiveInfinity;
        var vCost = VegetationCosts[to.Vegetation.Get(data)];
        if(vCost < 0) return Single.PositiveInfinity;
        if (to.Regime != movingRegime)
        {
            if(to.GetDomainUnits(Domain, data).Any()) return Single.PositiveInfinity;
            mult = HostileHexCostMult;
        }
        
        
        var cost = lfCost + vCost;
        if (data.RoadNetwork.Roads.TryGetValue(from.GetIdEdgeKey(to), out var road))
        {
            cost = Mathf.Min(cost, RoadCosts[road.Get(data)]);
        }
        return cost * mult;
    }
    
    public override void MoveThru(Hex hex, Unit unit, ProcedureKey key)
    {
        hex.SetRegime(unit.Regime, key);
    }
    
    private void ModifyForCombat(CombatModifier modifier, bool attacker,
        HexGeneralData data)
    {
        var effect = GetDamageMult(modifier.Hex, data,
                attacker);
        var mod = attacker 
            ? modifier.DamageToAttacker 
            : modifier.DamageToDefender;
        
        mod.AddMult(effect, "Move Type Terrain Effect");
    }
    
    public override void ModifyAsAttacker(CombatModifier modifier, 
        HexGeneralData data)
    {
        ModifyForCombat(modifier, true, data);
    }

    public override void ModifyAsDefender(CombatModifier modifier, HexGeneralData data)
    {
        ModifyForCombat(modifier, false, data);
    }

    public override bool ValidDest(Hex hex, Regime movingRegime, HexGeneralData data)
    {
        var land = data.ModelPredefs.Domains.LandDomain;
        if (hex.Landform.Get(data).IsLand == false) return false;
        if (hex.Full(land, data))
        {
            return false;
        }

        if (hex.Regime != movingRegime && hex.GetDomainUnits(land, data).Any())
        {
            return false;
        }

        return true;
    }

    public float GetDamageMult(Hex hex, HexGeneralData data, bool attacking)
    {
        if (attacking)
        {
            return AttackTerrainDamageModel.GetMult(hex, data);
        }

        return DefendTerrainDamageModel.GetMult(hex, data);
    }
    
    public override HashSet<Hex> GetMoveRadius(Hex start, Regime movingRegime, 
        float maxCost,
        HexGeneralData data)
    {
        var radius = PathFinder<Hex>.FloodFill(
            start, h => h.GetNeighbors(data),
            (h, g) => GetMoveCost(h, g, movingRegime, data),
            maxCost);
        return radius.Where(h => ValidDest(h, movingRegime, data))
            .ToHashSet();
    }
}