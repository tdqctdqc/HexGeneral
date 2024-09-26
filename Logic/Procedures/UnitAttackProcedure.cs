using System.Linq;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using GodotUtilities.Server;
using HexGeneral.Game;
using HexGeneral.Game.Components;

namespace HexGeneral.Logic.Procedures;

public class UnitAttackProcedure(ERef<Unit> unit, ERef<Unit> targetUnit, float damageToUnit, float damageToTarget, float damageToUnitOrg, float damageToTargetOrg) : GodotUtilities.Server.Procedure
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public ERef<Unit> TargetUnit { get; private set; } = targetUnit;
    public float DamageToUnit { get; private set; } = damageToUnit;
    public float DamageToUnitOrg { get; private set; } = damageToUnitOrg;
    public float DamageToTarget { get; private set; } = damageToTarget;
    public float DamageToTargetOrg { get; private set; } = damageToTargetOrg;

    public override void Handle(ProcedureKey key)
    {
        var unit = Unit.Get(key.Data);
        unit.Components.Get<AttackCountComponent>(key.Data)
            .SpendAttack(key);
        unit.IncrementHitpoints(-DamageToUnit, key);
        var targetUnit = TargetUnit.Get(key.Data);
        targetUnit.IncrementHitpoints(-DamageToTarget, key);

        var unitOrg = unit.Components.Get<OrganizationComponent>(key.Data);
        unitOrg.IncrementOrganization(-DamageToUnitOrg, key);
        var targetOrg = targetUnit.Components.Get<OrganizationComponent>(key.Data);
        targetOrg.IncrementOrganization(-DamageToTargetOrg, key);
        
        
        foreach (var c in unit.Components.OfType<IUnitCombatComponent>(key.Data))
        {
            c.AfterCombat(key);
        }
        foreach (var c in targetUnit.Components.OfType<IUnitCombatComponent>(key.Data))
        {
            c.AfterCombat(key);
        }
        
        var notices = key.Data.Data().Notices;
        notices.UnitAltered?.Invoke(unit);
        notices.UnitAltered?.Invoke(targetUnit);
        key.Data.Data().Notices.UnitAttacked?.Invoke(this);
    }
}