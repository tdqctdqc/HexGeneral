using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Ui;
using HexGeneral.Data.Components;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Client.Ui;

public class UnitBuildPortAction : MouseAction
{
    private readonly SettingsOption<Unit> _selectedUnit;
    private readonly HexGeneralClient _client;
    private readonly MapOverlayDrawer _overlay;
    public UnitBuildPortAction(MouseButtonMask button,
        SettingsOption<Unit> selectedUnit,
        HexGeneralClient client) : base(button)
    {
        _selectedUnit = selectedUnit;
        _client = client;
        _overlay = new MapOverlayDrawer((int)GraphicsLayers.Debug,
            _client.GetComponent<MapGraphics>);
    }

    private bool ValidHex(Hex h)
    {
        var unit = _selectedUnit.Value;
        if (unit is null) return false;
        var unitHex = unit.GetHex(_client.Data);
        return h.Landform.Get(_client.Data).IsLand == false
               && h.GetNeighbors(_client.Data).Contains(unitHex);
    }

    protected override void MouseDown(InputEventMouse m)
    {
        
    }

    protected override void MouseHeld(InputEventMouse m)
    {
        _overlay.Clear();
        var mouseHex = MouseOverHandler.FindMouseOverHex(_client);
        if (ValidHex(mouseHex) == false)
        {
            return;
        }
        var port = _client.Data.ModelPredefs.Buildings.Port;
        var unitHex = _selectedUnit.Value.GetHex(_client.Data);
        

        _overlay.Draw(mb =>
        {
            mb.AddArrow(unitHex.WorldPos(), mouseHex.WorldPos(), .25f, Colors.Orange);
            mb.AddArrow(unitHex.WorldPos(), mouseHex.WorldPos(), .2f, Colors.White);
        }, Vector2.Zero);
        var e = _selectedUnit.Value.Components.Get<EngineerEntityComponent>(_client.Data);
        _overlay.AddNode(new ConstructionTooltip(e, port, mouseHex, _client),
            mouseHex.WorldPos());
    }

    protected override void MouseUp(InputEventMouse m)
    {
        _overlay.Clear();
        var mouseHex = MouseOverHandler.FindMouseOverHex(_client);

        if (ValidHex(mouseHex) == false)
        {
            return;
        }
        var port = _client.Data.ModelPredefs.Buildings.Port;
        var progress = 
            Mathf.Min(port.EngineerPointCost,
                _selectedUnit.Value.Components
                    .Get<EngineerEntityComponent>(_client.Data)
                    .CurrentEngineerPoints);
        if (mouseHex.TryGetLocation(_client.Data, out var _) == false)
        {
            var loc = new Location(0, mouseHex.MakeRef(),
                new List<ModelIdRef<BuildingModel>>());
            var locCom = new EntityCreationCommand<Location>(loc);
            _client.SubmitCommand(locCom);
        }
        
        var proc = new WorkOnBuildingProcedure(
            mouseHex.MakeRef(),
            _selectedUnit.Value.MakeRef(), 
            progress, 
            port.MakeIdRef<ConstructableBuildingModel>(_client.Data));
        var command = new DoProcedureCommand(proc);
        _client.SubmitCommand(command);
    }

    public override void Clear()
    {
        _overlay.Clear();
    }
}