using System;
using System.Linq;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Client.Ui;
using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Game.Client;

public class UnitSelectAction(
    MouseButtonMask button,
    HexGeneralClient client,
    Action<Unit> selectAction)
    : MouseAction(button)
{
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
        var hex = MouseOverHandler.FindMouseOverHex(client);
        if (hex is null)
        {
            return;
        }
        
        var hexWorldPos = hex.WorldPos();
        var unitGraphics = client.GetComponent<MapGraphics>()
            .Units;
        var closest = unitGraphics.GetClosestUnitInHex(hex, mousePos, client);
        selectAction(closest);
    }

    public override void Clear()
    {
    }
}