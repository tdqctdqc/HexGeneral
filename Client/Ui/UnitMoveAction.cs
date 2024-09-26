using System.Collections.Generic;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Components;

namespace HexGeneral.Client.Ui;

public class UnitMoveAction : MouseAction
{
    private SettingsOption<Unit> _selectedUnit;
    private MapOverlayDrawer _radiusOverlay, _pathOverlay;
    private HexGeneralClient _client;
    public UnitMoveAction(MapOverlayDrawer radiusOverlay,
        MapOverlayDrawer pathOverlay,
        SettingsOption<Unit> selectedUnit,
        HexGeneralClient client,
        MouseButtonMask button) : base(button)
    {
        _selectedUnit = selectedUnit;
        _radiusOverlay = radiusOverlay;
        _pathOverlay = pathOverlay;
        _client = client;
        _selectedUnit.SettingChanged.Subscribe(v =>
        {
            DrawForSelectedUnit(v.newVal);
        });
    }

    private void DrawForSelectedUnit(Unit unit)
    {
        _radiusOverlay.Clear();
        _pathOverlay.Clear();
        if (unit is null 
            || unit.Components.Get<MoveCountComponent>(_client.Data).CanMove() == false)
        {
            return;
        }
        var comp = unit.Components.Get<IMoveComponent>(_client.Data);
        comp.DrawRadius(unit, _radiusOverlay, _client.Data);
    }
    protected override void MouseDown(InputEventMouse m)
    {
        
    }

    protected override void MouseHeld(InputEventMouse m)
    {
        _pathOverlay.Clear();

        var u = _selectedUnit.Value;
        if (u is null)
        {
            return;
        }

        var hex = MouseOverHandler.FindMouseOverHex(_client);
        if (hex is null) return;

        var comp = u.Components.Get<IMoveComponent>(_client.Data);
        comp.DrawPath(u, hex, _pathOverlay, _client.Data);
    }

    protected override void MouseUp(InputEventMouse m)
    {
        _pathOverlay.Clear();
        var unit = _selectedUnit.Value;

        if (unit is null)
        {
            return;
        }

        var moveComp = unit.Components.Get<IMoveComponent>(_client.Data);
        var moveCountComp = unit.Components.Get<MoveCountComponent>(_client.Data);

        if (moveCountComp.CanMove() == false)
        {
            return;
        }
        
        var mouseHex = MouseOverHandler.FindMouseOverHex(_client);
        if (mouseHex is null)
        {
            return;
        }
        var inner = moveComp.GetMoveCommand(unit, mouseHex, _client);
        if (inner is not null)
        {
            _radiusOverlay.Clear();
            var com = CallbackCommand.Construct(inner,
                () => DrawForSelectedUnit(unit), _client);
            _client.SubmitCommand(com);
        }
    }
}