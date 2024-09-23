using System.Collections.Generic;
using GodotUtilities.GameData;
using HexGeneral.Game.Components;

namespace HexGeneral.Game;

public class UnitModel : Model
{
    public float HitPoints { get; private set; }
    public float Organization { get; private set; }
    public int AmmoCap { get; private set; }
    public int Range { get; private set; }
    public float HardAttack { get; private set; }
    public float SoftAttack { get; private set; }
    public float Hardness { get; private set; }
    public float RecruitCost { get; private set; }
    public float IndustrialCost { get; private set; }
    public float AmmoCost { get; private set; }
    public float MovePoints { get; private set; }
    public MoveType MoveType { get; private set; }
    public AttackType AttackType { get; private set; }

    public Unit Instantiate(Regime regime, HexGeneralData data)
    {
        var u = Unit.Instantiate(this, regime, data);
        return u;
    }
}