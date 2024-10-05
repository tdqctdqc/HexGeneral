using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using GodotUtilities.Server;
using GodotUtilities.Ui;
using HexGeneral.Data.Components;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Game.Components;
using HexGeneral.Logic.Procedure;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Client.Ui;

public class EmbarkShipAction : MouseAction
{
    private SettingsOption<Unit> _selectedUnit;
    private MapOverlayDrawer _radiusOverlay, _pathOverlay;
    private HexGeneralClient _client;
    public EmbarkShipAction(
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

    private bool Valid()
    {
        var u = _selectedUnit.Value;
        if (u is null)
        {
            return false;
        }

        if (u.Components.Get<EmbarkedComponent>(_client.Data)
            is not null)
        {
            return false;
        }
        
        var mc = u.Components.Get<IMoveComponent>(_client.Data);
        if (mc.GetActiveMoveType(_client.Data).Domain 
            != _client.Data.ModelPredefs.Domains.LandDomain)
        {
            return false;
        }
        
        var moveCountComp = u.Components.Get<MoveCountComponent>(_client.Data);
        if (moveCountComp.CanMove() == false)
        {
            return false;
        }

        return true;
    }
    private bool ValidHex(Hex n)
    {
        var sea = _client.Data.ModelPredefs.Domains.SeaDomain;
        return n.Landform.Get(_client.Data).IsLand == false
               && n.Full(sea, _client.Data) == false
               && n.TryGetLocation(_client.Data, out var loc)
               && loc.HasBuilding<PortBuilding>(_client.Data);
    }
    private void DrawForSelectedUnit(Unit unit)
    {
        _radiusOverlay.Clear();
        if (Valid() == false) return;
        var unitHex = unit.GetHex(_client.Data);
        
        _radiusOverlay.Draw(mb =>
        {
            foreach (var n in unitHex.GetNeighbors(_client.Data))
            {
                if (ValidHex(n))
                {
                    mb.DrawHex(n.WorldPos(), 1f,
                        Colors.Red.Tint(.5f));
                }
            }
        }, Vector2.Zero);
    }
    protected override void MouseDown(InputEventMouse m)
    {
        
    }

    protected override void MouseHeld(InputEventMouse m)
    {
        _pathOverlay.Clear();
        if (Valid() == false) return;
        var u = _selectedUnit.Value;
        var unitHex = u.GetHex(_client.Data);
        var mouseHex = MouseOverHandler.FindMouseOverHex(_client);

        if (ValidHex(mouseHex) == false)
        {
            return;
        }
        
        _pathOverlay.Draw(mb =>
        {
            mb.AddArrow(unitHex.WorldPos(), mouseHex.WorldPos(),
                .25f, Colors.AliceBlue);
            mb.AddArrow(unitHex.WorldPos(), mouseHex.WorldPos(),
                .2f, Colors.White);
        }, Vector2.Zero);
    }

    protected override void MouseUp(InputEventMouse m)
    {
        _pathOverlay.Clear();
        if (Valid() == false) return;
        var unit = _selectedUnit.Value;
        var mouseHex = MouseOverHandler.FindMouseOverHex(_client);
        

        if (ValidHex(mouseHex) == false)
        {
            return;
        }

        Action callback = () =>
        {
            DrawForSelectedUnit(unit);
            _selectedUnit.Set(unit);
        };

        var transport = _client.Data.ModelPredefs.Mobilizers.Transport;

        var embark = new EmbarkProcedure(unit.MakeRef(),
            transport.MakeIdRef<Mobilizer>(_client.Data),
            mouseHex.MakeRef());
        var inner = new DoProcedureCommand(embark);
        var com = CallbackCommand.Construct(inner, callback, _client);
        _client.SubmitCommand(com);
    }

    public override void Clear()
    {
        _radiusOverlay.Clear();
        _pathOverlay.Clear();
    }
}