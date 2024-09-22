using System.Linq;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game.Components;
using HexGeneral.Logic.Procedure;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game.Logic;

public static class CombatLogic
{
    public static void HandleAttack(Unit unit, Unit targetUnit, 
        LogicKey key)
    {
        var data = key.Data.Data();
        var targetHex = targetUnit.GetHex(data);
        var unitModel = unit.UnitModel.Get(data);
        var targetUnitModel = targetUnit.UnitModel.Get(data);

        var unitHex = unit.GetHex(data);
        var dist = unitHex.Coords.GetHexDistance(targetHex.Coords);
        if (dist > unitModel.Range) return;
        
        var targetStartHp = targetUnit.CurrentHitPoints;
        var unitModifier = CombatModifier.Construct(unit, targetUnit, true, targetHex, data);
        var targetUnitModifier = CombatModifier.Construct(targetUnit, unit, false, targetHex, data);
        
        var dmgToTarget = unitModifier.GetDamageAgainst(targetUnitModifier, data);
        
        var dmgToUnit = 0f;
        if (dist <= targetUnitModel.Range)
        {
            dmgToUnit = targetUnitModifier.GetDamageAgainst(unitModifier,  data);
        }


        key.SendMessage(new UnitAttackProcedure(unit.MakeRef(), targetUnit.MakeRef(),
            dmgToUnit, dmgToTarget, dmgToUnit, dmgToTarget));

        DoUnitDestroyedCheck(unit, key);
        DoUnitDestroyedCheck(targetUnit, key);
        
        DoUnitRetreatCheck(targetUnit, dmgToTarget, targetStartHp, key);
    }

    public static void DoUnitDestroyedCheck(Unit unit, LogicKey key)
    {
        if (unit.CurrentHitPoints <= 0f)
        {
            key.SendMessage(new UnitDestroyedProcedure(unit.MakeRef()));
        }
    }

    public static void DoUnitRetreatCheck(Unit unit, float damage, 
        float startHp, LogicKey key)
    {
        var data = key.Data.Data();
        var regime = unit.Regime;
        if (damage >= startHp / 2f)
        {
            var unitHex = unit.GetHex(data); 
            var retreat = FloodFill<Hex>.FloodFillToRadiusTilFirst(
                unitHex, 3,
                h => h.GetNeighbors(data).Where(n => n.Regime == regime),
                h => h.Full(data) == false, out _,
                out var distance);
            if (retreat is null)
            {
                key.SendMessage(new UnitDestroyedProcedure(unit.MakeRef()));
            }
            else
            {
                key.SendMessage(new UnitRetreatProcedure(
                    unitHex.MakeRef(), retreat.MakeRef(), unit.MakeRef(), distance));
            }
        }
    }
    
    public static float
        GetDamageAgainst(Unit damager, 
        Unit damagee, Hex hex, 
        bool damagerAttacking, 
        HexGeneralData data)
    {
        var damagerModifier = CombatModifier.Construct(damager, 
            damagee, damagerAttacking, hex, data);
        var damageeModifier = CombatModifier.Construct(damagee, 
            damager, damagerAttacking == false, hex, data);

        return damagerModifier.GetDamageAgainst(damageeModifier, data);
    }
    
    

    public static float GetEffectiveAttackRatio(Unit attacker,
        HexGeneralData data)
    {
        var atkModel = attacker.UnitModel.Get(data);

        var atkOrgRatio = attacker.CurrentOrganization / atkModel.Organization;
        var ammoEffect = atkModel.AmmoCap > 0 && attacker.CurrentAmmo == 0f
            ? .25f
            : 1f;
        return atkOrgRatio * ammoEffect;
    }
}