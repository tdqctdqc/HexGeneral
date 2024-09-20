using Godot;
using GodotUtilities.CSharpExt;
using HexGeneral.Game.Logic;

namespace HexGeneral.Game.Client.Graphics;

public partial class AttackTooltip : Node2D
{
    public override void _Ready()
    {
        Scale = Vector2.One * .015f;
    }

    public void DrawInfo(Hex hex, Unit unit, Unit targetUnit, HexGeneralData data)
    {
        var damageToTarget = CombatLogic.GetDamageAgainst(unit, targetUnit,
            hex, true, data);
        var damageMultToTarget = targetUnit.UnitModel.Get(data)
            .MoveType.GetDamageMult(hex, data, false);
        
        var damageToUnit = CombatLogic.GetDamageAgainst(targetUnit, unit,
            hex, false, data);
        var damageMultToUnit = unit.UnitModel.Get(data)
            .MoveType.GetDamageMult(hex, data, true);

        ((Label)FindChild("Attacker")).Text = $"{damageToUnit} ({damageMultToUnit.RoundTo2Digits()})";
        ((Label)FindChild("Defender")).Text = $"{damageToTarget} ({damageMultToTarget.RoundTo2Digits()})";
    }
}