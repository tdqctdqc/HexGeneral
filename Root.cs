using Godot;
using System;
using StartScreen = HexGeneral.Client.Ui.StartScreen;

namespace HexGeneral.Game;
public partial class Root : Node
{
	public static Root I { get; private set; }
	public override void _Ready()
	{
		if (I is null)
		{
			I = this;
		}
		else
		{
			throw new Exception();
		}
		AddChild(new StartScreen());
	}

	public override void _Process(double delta)
	{
	}
}
