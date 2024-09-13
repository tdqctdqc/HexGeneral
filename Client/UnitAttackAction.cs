using System.Linq;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Client.Ui;
using HexGeneral.Game.Client.Command;

namespace HexGeneral.Game.Client;

public class UnitAttackAction : MouseAction
{
    private SettingsOption<Unit> _selectedUnit;
    private MapOverlayDrawer _arrowOverlay;
    private HexGeneralClient _client;
    public UnitAttackAction(MouseButtonMask button, SettingsOption<Unit> selectedUnit, MapOverlayDrawer arrowOverlay, HexGeneralClient client) : base(button)
    {
        _selectedUnit = selectedUnit;
        _arrowOverlay = arrowOverlay;
        _client = client;
    }

    protected override void MouseDown(InputEventMouse m)
    {
    }

    protected override void MouseHeld(InputEventMouse m)
    {
        var unit = _selectedUnit.Value;
        if (unit is null || unit.Attacked)
        {
            _arrowOverlay.Clear();
            return;
        }
        
        var regime = unit.Regime.Get(_client.Data);

        var pos = _client.GetComponent<CameraController>().GetGlobalMousePosition();
        var mouseHex = MouseOverHandler.FindTwoClosestHexes(pos,
            _client.Data.Map);
        if (mouseHex.closest is null)
        {
            _arrowOverlay.Clear();
            return;
        }

        _arrowOverlay.Clear();
        var uHex = _client.Data.MapUnitHolder
            .UnitPositions[unit.MakeRef()];
        var unitHex = _client.Data.Map.Hexes[uHex.Coords];
        var hex = mouseHex.closest;
        if (hex.Regime == regime)
        {
            return;
        }
        if (hex.GetNeighbors(_client.Data).Contains(unitHex) == false)
        {
            return;
        }
        
        _arrowOverlay.Draw(mb =>
        {
            mb.AddArrow(unitHex.WorldPos(), hex.WorldPos(), .25f, Colors.White);
            mb.AddArrow(unitHex.WorldPos(), hex.WorldPos(), .2f, Colors.Red);
        }, Vector2.Zero);
    }

    protected override void MouseUp(InputEventMouse m)
    {
        var unit = _selectedUnit.Value;
        if (unit is null || unit.Attacked)
        {
            _arrowOverlay.Clear();
            return;
        }
        
        var regime = unit.Regime.Get(_client.Data);

        var pos = _client.GetComponent<CameraController>().GetGlobalMousePosition();
        var mouseHex = MouseOverHandler.FindTwoClosestHexes(pos,
            _client.Data.Map);
        if (mouseHex.closest is null)
        {
            _arrowOverlay.Clear();
            return;
        }

        _arrowOverlay.Clear();
        var unitHex = unit.GetHex(_client.Data);
        var hex = mouseHex.closest;
        if (hex.Regime == regime)
        {
            return;
        }
        if (hex.GetNeighbors(_client.Data).Contains(unitHex) == false)
        {
            return;
        }

        var com = new UnitAttackCommand(unit.MakeRef(), hex.Coords, unitHex.Coords);
        _client.SubmitCommand(com);
    }
}