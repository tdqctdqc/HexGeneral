using System;
using System.Linq;
using Godot;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Client.Ui;
using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Game.Client;

public class HexSelectAction(
    MouseButtonMask button,
    HexGeneralClient _client,
    Action<Hex> selectAction)
    : MouseAction(button)
{
    private Action<Hex> _selectAction = selectAction;
    protected override void MouseDown(InputEventMouse m)
    {
    }

    protected override void MouseHeld(InputEventMouse m)
    {
    }

    protected override void MouseUp(InputEventMouse m)
    {
        var hex = MouseOverHandler.FindMouseOverHex(_client);
        _selectAction?.Invoke(hex);
    }

    public override void Clear()
    {
        
    }
}