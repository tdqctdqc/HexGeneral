using Godot;
using System;
using System.Linq;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using StartScreen = HexGeneral.Client.Ui.StartScreen;

namespace HexGeneral.Game;
public partial class Root : Node
{
	public static Root I { get; private set; }
	private GameClient _client;
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

	public void SetClient(GameClient c)
	{
		_client = c;
	}

	public override void _Input(InputEvent @event)
	{
		_client?._Input(@event);
	}
}


