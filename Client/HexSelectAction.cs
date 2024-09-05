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
    HexGeneralClient client,
    MouseOverHandler mouseOverHandler,
    Action<Hex> selectAction)
    : MouseAction(button)
{
    private MouseOverHandler _mouseOverHandler = mouseOverHandler;
    private Action<Hex> _selectAction = selectAction;
    protected override void MouseDown(InputEventMouse m)
    {
    }

    protected override void MouseHeld(InputEventMouse m)
    {
    }

    protected override void MouseUp(InputEventMouse m)
    {
        var mousePos = client.GetComponent<CameraController>()
            .GetGlobalMousePosition();
        var (hex, _) = MouseOverHandler.FindTwoClosestHexes(mousePos,
            client.Data.Map);
        _selectAction?.Invoke(hex);
    }
}