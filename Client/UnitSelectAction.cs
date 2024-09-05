using System;
using System.Linq;
using Godot;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Client.Ui;
using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Game.Client;

public class UnitSelectAction(
    MouseButtonMask button,
    HexGeneralClient client,
    MouseOverHandler mouseOverHandler,
    Action<Unit> selectAction)
    : MouseAction(button)
{
    private MouseOverHandler _mouseOverHandler = mouseOverHandler;
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
        if (hex is null)
        {
            return;
        }
        
        var hexWorldPos = hex.WorldPos();
        if (client.Data.MapUnitHolder.HexLandUnits.TryGetValue(hex.Coords, out var hexUnits)
            && hexUnits.Count > 0)
        {
            var unitGraphics = client.GetComponent<MapGraphics>()
                .Units;
            var offsets = UnitGraphics.GraphicOffsetsByNumUnits[hexUnits.Count - 1];

            var closest = hexUnits
                .Select((u, i) => (u, i))
                .MinBy(v => mousePos.DistanceTo(hexWorldPos + offsets[v.i]))
                .u.Get(client.Data);
            selectAction(closest);
        }
        else
        {
            selectAction(null);
        }
        
    }
}