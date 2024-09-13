using System.Linq;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Logic.Procedure;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game.Logic;

public static class CombatLogic
{
    public static void HandleAttack(Unit unit, Hex targetHex, LogicKey key)
    {
        var data = key.Data.Data();
        var targetUnit = targetHex.GetUnitRefs(data)
            .GetRandomElement().Get(data);
        var unitModel = unit.UnitModel.Get(data);
        var targetUnitModel = targetUnit.UnitModel.Get(data);

        var startHp = unit.CurrentHitPoints;
        var targetStartHp = targetUnit.CurrentHitPoints;
        
        var dmgToTarget = GetDamageAgainst(unit, targetUnit, data);
        var dmgToUnit = GetDamageAgainst(targetUnit, unit, data);

        key.SendMessage(new UnitAttackedProcedure(unit.MakeRef(), targetUnit.MakeRef(),
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
        GetDamageAgainst(Unit attacker, 
        Unit defender, HexGeneralData data)
    {
        var atkModel = attacker.UnitModel.Get(data);
        var defModel = defender.UnitModel.Get(data);
        var soft = atkModel.SoftAttack * (1f - defModel.Hardness);
        var hard = atkModel.HardAttack * defModel.Hardness;

        var atkOrgRatio = attacker.CurrentOrganization / atkModel.Organization;
        var ammoEffect = atkModel.AmmoCap > 0 && attacker.CurrentAmmo == 0f
            ? .25f
            : 1f;
        return (soft + hard) * atkOrgRatio * ammoEffect;
    }
}