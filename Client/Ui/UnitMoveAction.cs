using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.Graphics;
using GodotUtilities.Server;
using GodotUtilities.Ui;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Game.Components;

namespace HexGeneral.Client.Ui;

public class UnitMoveAction : MouseAction
{
    private SettingsOption<Unit> _selectedUnit;
    private MapOverlayDrawer _radiusOverlay, _pathOverlay;
    private HexGeneralClient _client;
    public UnitMoveAction(
        SettingsOption<Unit> selectedUnit,
        HexGeneralClient client,
        MouseButtonMask button) : base(button)
    {
        _client = client;
        _selectedUnit = selectedUnit;
        _pathOverlay = new MapOverlayDrawer((int)GraphicsLayers.Debug, _client.GetComponent<MapGraphics>);
        _radiusOverlay = new MapOverlayDrawer(
            (int)GraphicsLayers.Debug, 
                _client.GetComponent<MapGraphics>);

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
            || unit.Components.Get<MoveCountComponent>(_client.Data)
                .CanMove() == false)
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
        comp.GetActiveMoveType(_client.Data).DrawPath(u, hex, _pathOverlay, _client.Data);
    }

    protected override void MouseUp(InputEventMouse m)
    {
        _pathOverlay.Clear();
        var unit = _selectedUnit.Value;

        if (unit is null)
        {
            return;
        }

        var mouseHex = MouseOverHandler.FindMouseOverHex(_client);
        if (mouseHex is null)
        {
            return;
        }

        Action<Command> callback = c =>
        {
            _radiusOverlay.Clear();
            var com = CallbackCommand.Construct(c,
                () =>
                {
                    DrawForSelectedUnit(unit);
                    _selectedUnit.Set(unit);
                }, _client);
            _client.SubmitCommand(com);
        };
        var moveComp = unit.Components.Get<IMoveComponent>(_client.Data);

        moveComp.TryMoveCommand(unit, mouseHex, callback, _client);
    }

    public override void Clear()
    {
        _radiusOverlay.Clear();
        _pathOverlay.Clear();
    }
}