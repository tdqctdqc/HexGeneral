using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures.PathFinder;
using GodotUtilities.DataStructures.RefAction;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Client.Ui;
using HexGeneral.Game.Client.Command;

namespace HexGeneral.Game.Client;

public class UnitMoveAction : MouseAction
{
    private MapOverlayDrawer _pathOverlay, _radiusOverlay;
    private HexGeneralClient _client;
    private SettingsOption<Unit> _selectedUnit;
    private Hex _cachedHex;
    private HashSet<Hex> _moveRadius;
    
    public UnitMoveAction(MouseButtonMask button, 
        SettingsOption<Unit> selectedUnit,
        MapOverlayDrawer pathOverlay, HexGeneralClient client, MapOverlayDrawer radiusOverlay) : base(button)
    {
        _selectedUnit = selectedUnit;
        selectedUnit.SettingChanged.Subscribe(v =>
        {
            _cachedHex = null;
            SetMoveRadius();
        });
        _pathOverlay = pathOverlay;
        _client = client;
        _radiusOverlay = radiusOverlay;
        _moveRadius = new HashSet<Hex>();
    }

    private void SetMoveRadius()
    {
        _radiusOverlay.Clear();
        _moveRadius.Clear();
        var unit = _selectedUnit.Value;
        if (unit is null || unit.Moved) return;
        
        var unitHex = unit.GetHex(_client.Data);
        var regime = unit.Regime.Get(_client.Data);
        var unitModel = unit.UnitModel.Get(_client.Data);
        var moveType = unitModel.MoveType;
        var hexTris = ShapeBuilder.GetHex(Vector2.Zero, 1f);
        _moveRadius = moveType.GetMoveRadius(unitHex, regime, unitModel.MovePoints,
            _client.Data);
        _radiusOverlay.Draw(mb =>
        {
            foreach (var hex in _moveRadius)
            {
                mb.AddTris(hexTris.Select(p => p + hex.WorldPos()), Colors.White.Tint(.5f));
            }
        }, Vector2.Zero);
    }
    protected override void MouseDown(InputEventMouse m)
    {
        
    }

    protected override void MouseHeld(InputEventMouse m)
    {
        var unit = _selectedUnit.Value;
        if (unit is null || unit.Moved)
        {
            _pathOverlay.Clear();
            return;
        }
        
        var regime = unit.Regime.Get(_client.Data);

        var pos = _client.GetComponent<CameraController>().GetGlobalMousePosition();
        var mouseHex = MouseOverHandler.FindTwoClosestHexes(pos,
            _client.Data.Map);
        if (mouseHex.closest is null)
        {
            _pathOverlay.Clear();
            return;
        }

        if (mouseHex.close == _cachedHex)
        {
            return;
        }
        _pathOverlay.Clear();
        var unitHex = unit.GetHex(_client.Data);
        _cachedHex = mouseHex.closest;
        var path = unit.UnitModel.Get(_client.Data).MoveType
            .GetPath(unitHex, _cachedHex, regime, _client.Data, out var cost);
        if (path is null)
        {
            return;
        }
        _pathOverlay.Draw(mb =>
        {
            for (var i = 0; i < path.Count - 1; i++)
            {
                var from = path[i];
                var to = path[i + 1];
                mb.AddArrow(from.WorldPos(), to.WorldPos(), .25f, Colors.Black);
                mb.AddArrow(from.WorldPos(), to.WorldPos(), .2f, Colors.White);
            }
        }, Vector2.Zero);
    }

    protected override void MouseUp(InputEventMouse m)
    {
        _pathOverlay.Clear();
        var unit = _selectedUnit.Value;
        
        if (unit is null || unit.Moved || _cachedHex is null)
        {
            return;
        }
        var unitHex = unit.GetHex(_client.Data);
        if (unitHex == _cachedHex) return;
        var player = _client.Data.PlayerHolder
            .PlayerByGuid[_client.PlayerGuid].Get(_client.Data);
        if (player.Regime != unit.Regime) return;
        var regime = unit.Regime.Get(_client.Data);
        
        var path = unit.UnitModel.Get(_client.Data).MoveType
            .GetPath(unitHex, _cachedHex, regime, _client.Data, out var cost);
        if (path is null)
        {
            return;
        }

        var inner = new UnitMoveCommand(unit.MakeRef(), 
            path.Select(h => h.MakeRef()).ToList(), cost);
        var com = CallbackCommand.Construct(inner,
            SetMoveRadius, _client);
        _client.SubmitCommand(com);
    }
}