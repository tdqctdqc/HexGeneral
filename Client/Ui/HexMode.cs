using System.Collections.Generic;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.Ui;
using HexGeneral.Client.Ui;

namespace HexGeneral.Game.Client;

public class HexMode : UiMode
{
    public SettingsOption<Hex> SelectedHex { get; private set; }
    public HexMode(HexGeneralClient client, string name) : base(client, name)
    {
        SelectedHex = new DefaultSettingsOption<Hex>("Selected Hex", null);
        var hexSelectMode = new MouseMode(
            new List<MouseAction>
            {
                new HexSelectAction(MouseButtonMask.Left, 
                    client, 
                    SelectedHex.Set)
            });
        MouseMode.Set(hexSelectMode);
    }

    public override void Process(float delta)
    {
    }

    public override void HandleInput(InputEvent e)
    {
        if (e is InputEventMouse m)
        {
            MouseMode.Value?.HandleInput(m);
        }
    }

    public override void Enter()
    {
    }

    public override void Clear()
    {
    }

    public override Control GetControl(GameClient client)
    {
        return new HexPanel((HexGeneralClient)client, this);
    }
}