using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using GodotUtilities.Server;
using HexGeneral.Game;

namespace HexGeneral.Logic.Procedures;

public class UnitAttackedProcedure(ERef<Unit> unit, ERef<Unit> targetUnit, float damageToUnit, float damageToTarget, float damageToUnitOrg, float damageToTargetOrg) : GodotUtilities.Server.Procedure
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
        unit.MarkHasAttacked(key);
        unit.IncrementHitpoints(-DamageToUnit, key);
        var targetUnit = TargetUnit.Get(key.Data);
        targetUnit.IncrementHitpoints(-DamageToTarget, key);
        
        unit.IncrementOrganization(-DamageToUnitOrg, key);
        targetUnit.IncrementOrganization(-DamageToTargetOrg, key);
        
        unit.IncrementAmmo(-1, key);
        targetUnit.IncrementAmmo(-1, key);
        
        key.Data.Data().Notices.UnitAttacked?.Invoke(this);
    }
}