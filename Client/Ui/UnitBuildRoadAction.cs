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

public class UnitBuildRoadAction : MouseAction
{
    private readonly SettingsOption<Unit> _selectedUnit;
    private readonly HexGeneralClient _client;
    private readonly MapOverlayDrawer _overlay;
    public UnitBuildRoadAction(MouseButtonMask button,
        SettingsOption<Unit> selectedUnit,
        HexGeneralClient client) : base(button)
    {
        _selectedUnit = selectedUnit;
        _client = client;
        _overlay = new MapOverlayDrawer((int)GraphicsLayers.Debug,
            _client.GetComponent<MapGraphics>);
    }

    private RoadModel GetValidRoadModel()
    {
        var unit = _selectedUnit.Value;
        if (unit is null)
        {
            return null;
        }

        if (unit.Components.Get<EngineerEntityComponent>(_client.Data)
                is EngineerEntityComponent e == false)
        {
            return null;
        }

        var unitHex = unit.GetHex(_client.Data);
        if (unitHex.Landform.Get(_client.Data).IsLand == false)
        {
            return null; 
        }
        
        var mouseHex = MouseOverHandler.FindMouseOverHex(_client);
        if (mouseHex is null) return null;
        if (mouseHex.Landform.Get(_client.Data).IsLand == false)
        {
            return null; 
        }

        if (mouseHex.Regime != unitHex.Regime) return null;

        if (unitHex.GetNeighbors(_client.Data)
                .Contains(mouseHex) == false)
        {
            return null;
        }

        var roadModels = _client.Data.Models.GetModels<RoadModel>()
            .OrderBy(r => r.Level).ToArray();

        var currPriority = _client.Data.RoadNetwork.Roads.TryGetValue(
            unitHex.GetIdEdgeKey(mouseHex), out var roadRef)
                ? roadRef.Get(_client.Data).Level
                : -1;
        var toBuild = roadModels.FirstOrDefault(
            r => r.Level > currPriority);
        if (toBuild is null)
        {
            return null;
        }

        return toBuild;
    }
    protected override void MouseDown(InputEventMouse m)
    {
        
    }

    protected override void MouseHeld(InputEventMouse m)
    {
        _overlay.Clear();
        var road = GetValidRoadModel();
        if (road is null) return;
        
        var unitHex = _selectedUnit.Value.GetHex(_client.Data);
        var mouseHex = MouseOverHandler.FindMouseOverHex(_client);
        _overlay.Draw(mb =>
        {
            mb.AddArrow(unitHex.WorldPos(), mouseHex.WorldPos(), .25f, Colors.Orange);
            mb.AddArrow(unitHex.WorldPos(), mouseHex.WorldPos(), .2f, Colors.White);
        }, Vector2.Zero);
        
        var e = _selectedUnit.Value.Components.Get<EngineerEntityComponent>(_client.Data);
        var edge = unitHex.GetIdEdgeKey(mouseHex);
        
        _overlay.AddNode(new ConstructionTooltip(e, road, edge, _client),
            mouseHex.WorldPos());
    }

    protected override void MouseUp(InputEventMouse m)
    {
        _overlay.Clear();
        var road = GetValidRoadModel();
        if (road is null) return;
        var progress = 
            Mathf.Min(road.EngineerPointCost,
                _selectedUnit.Value.Components
                    .Get<EngineerEntityComponent>(_client.Data)
                    .CurrentEngineerPoints);
        var unitHex = _selectedUnit.Value.GetHex(_client.Data);
        var mouseHex = MouseOverHandler.FindMouseOverHex(_client);
        var proc = new WorkOnRoadProcedure(unitHex.GetIdEdgeKey(mouseHex),
            _selectedUnit.Value.MakeRef(), 
            progress, 
            road.MakeIdRef(_client.Data));
        var command = new DoProcedureCommand(proc);
        _client.SubmitCommand(command);
    }

    public override void Clear()
    {
        _overlay.Clear();
    }
}