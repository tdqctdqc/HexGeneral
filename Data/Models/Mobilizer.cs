using System;
using System.Collections.Generic;
using GodotUtilities.GameData;
using HexGeneral.Game.Logic;

namespace HexGeneral.Game;

public class Mobilizer : Model
{
    public MoveType MoveType { get; private set; }
    public float DamageTakenMult { get; private set; }
    public float MovePoints { get; private set; }
    public float RecruitCost { get; private set; }
    public float IndustrialCost { get; private set; }
    public bool CanAttack { get; private set; }
    public bool CanDefend { get; private set; }
    public HashSet<MoveType> AllowedNativeMoveTypes { get; private set; }

    public void ModifyAsDefender(CombatModifier modifier,
        HexGeneralData data)
    {
        modifier.DamageToDefender.AddMult(DamageTakenMult, "Mobilizer damage taken mult");
        MoveType.ModifyAsDefender(modifier, data);
    }

    public void ModifyAsAttacker(CombatModifier modifier,
        HexGeneralData data)
    {
        if (CanAttack == false) throw new Exception();
        MoveType.ModifyAsAttacker(modifier, data);
    }

    public bool CanMobilize(Unit unit, HexGeneralData data)
    {
        var uMoveType = unit.UnitModel.Get(data).MoveType;
        return uMoveType.Domain == MoveType.Domain
               && AllowedNativeMoveTypes.Contains(uMoveType);
    }
}