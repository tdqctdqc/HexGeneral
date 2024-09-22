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

    public void DrawInfo(Hex hex, Unit unit, Unit targetUnit, 
        HexGeneralData data)
    {
        var unitModifier = CombatModifier.ConstructVerbose(unit, targetUnit, true, hex, data);
        var targetModifier = CombatModifier.ConstructVerbose(targetUnit, unit, false, hex, data);

        var damageToTarget = unitModifier.GetDamageAgainst(targetModifier, data);
        var damageToUnit = targetModifier.GetDamageAgainst(unitModifier, data);
        


        var atk = ((Label)FindChild("Attacker"));
        atk.Text = $"Expected Damage Taken: {damageToUnit}";
        atk.Text += unitModifier.Info();
        var def = ((Label)FindChild("Defender"));
        def.Text = $"Expected Damage Dealt: {damageToTarget}";
        def.Text += targetModifier.Info();
    }
}