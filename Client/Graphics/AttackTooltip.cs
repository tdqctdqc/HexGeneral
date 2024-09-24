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
        var modifier = new CombatModifier(unit, targetUnit, hex, true, data);

        var damageToTarget = modifier.DamageToDefender.Modify(0f);
        var damageToUnit = modifier.DamageToAttacker.Modify(0f);

        var atk = ((Label)FindChild("Attacker"));
        atk.Text = $"Expected Damage Taken: {damageToUnit}";
        atk.Text += modifier.DamageToAttacker.Print();
        var def = ((Label)FindChild("Defender"));
        def.Text = $"Expected Damage Dealt: {damageToTarget}";
        def.Text += modifier.DamageToDefender.Print();
    }
}