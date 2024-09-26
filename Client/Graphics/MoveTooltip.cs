using Godot;
using System;
using HexGeneral.Game;
using HexGeneral.Game.Components;
using HexGeneral.Game.Logic;

public partial class MoveTooltip : Node2D
{
	public override void _Ready()
	{
		Scale = Vector2.One * .015f;
	}

	public void DrawInfo(Unit unit, float cost, 
		HexGeneralData data)
	{
		var label = ((Label)FindChild("Cost"));
		var mp = unit.Components.Get<IMoveComponent>(data).GetMovePoints(data);
		var ratio = unit.Components.Get<MoveCountComponent>(data).MovePointRatioRemaining;

		label.Text = $"{cost} / {mp * ratio}";
		label.Modulate = cost <= mp * ratio ? Colors.Green : Colors.Red;
	}
}
