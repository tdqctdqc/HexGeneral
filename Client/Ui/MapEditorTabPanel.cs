using System.Linq;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.Ui;
using HexGeneral.Game;

namespace HexGeneral.Client.Ui;

public partial class MapEditorTabPanel : UiModeTabPanel
{
    private HexGeneralData _data;
    private MapEditorMode _mapEditorMode;
    private VBoxContainer _landform;
    private VBoxContainer _vegetation;
    private VBoxContainer _road;
    private readonly VBoxContainer _regime;

    public MapEditorTabPanel(MapEditorMode mode, HexGeneralData data) 
        : base(mode, null)
    {
        _mapEditorMode = mode;
        _data = data;
        _landform = new VBoxContainer();
        _landform.Name = "Landform";
        AddTab(_landform,
            mode.DrawLandformMode,
            () => true,
            DrawLandformInfo);
        
        _vegetation = new VBoxContainer();
        _vegetation.Name = "Vegetation";
        AddTab(_vegetation,
            mode.DrawVegetationMode,
            () => true,
            DrawVegetationInfo);

        _road = new VBoxContainer();
        _road.Name = "Road";
        AddTab(_road,
            mode.DrawRoadMode,
            () => true,
            DrawRoadInfo);
        
        _regime = new VBoxContainer();
        _regime.Name = "Regime";
        AddTab(_regime,
            mode.DrawRegimeMode,
            () => true,
            DrawRegimeInfo);
    }

    private void DrawRegimeInfo()
    {
        _regime.ClearChildren();
        var regimes = _data.Entities.GetAll<Regime>();
        var regimeScroll = new ItemListToken<Regime>(
            regimes, r => r.RegimeModel.Get(_data).Name,
            false);
        regimeScroll.SetExpand(5, 100);
        regimeScroll.Select(_mapEditorMode.SelectedRegime.Value);
        regimeScroll.JustSelected += () =>
        {
            _mapEditorMode.SelectedRegime.Set(regimeScroll.Selected.Single());
        };
        _regime.AddChild(regimeScroll.ItemList);
        
    }

    private void DrawRoadInfo()
    {
        _road.ClearChildren();
        var roads = _data.Models.GetModels<RoadModel>();
        var roadScroll = new ItemListToken<RoadModel>(
            roads, r => r.Name,
            false);
        roadScroll.SetExpand(5, 100);
        roadScroll.Select(_mapEditorMode.SelectedRoadModel.Value);
        roadScroll.JustSelected += () =>
        {
            _mapEditorMode.SelectedRoadModel.Set(roadScroll.Selected.Single());
        };
        _road.AddChild(roadScroll.ItemList);
    }

    private void DrawLandformInfo()
    {
        _landform.ClearChildren();
        var landforms = _data.Models.GetModels<Landform>();
        var lfScroll = new ItemListToken<Landform>(
            landforms, r => r.Name,
            false);
        lfScroll.SetExpand(5, 100);
        lfScroll.Select(_mapEditorMode.SelectedLandform.Value);
        lfScroll.JustSelected += () =>
        {
            _mapEditorMode.SelectedLandform.Set(lfScroll.Selected.Single());
        };
        _landform.AddChild(lfScroll.ItemList);

    }
    private void DrawVegetationInfo()
    {
        _vegetation.ClearChildren();
        var vegetations = _data.Models.GetModels<Vegetation>();
        var vScroll = new ItemListToken<Vegetation>(
            vegetations, r => r.Name,
            false);
        vScroll.SetExpand(5, 100);
        vScroll.Select(_mapEditorMode.SelectedVegetation.Value);
        vScroll.JustSelected += () =>
        {
            _mapEditorMode.SelectedVegetation.Set(vScroll.Selected.Single());
        };
        _vegetation.AddChild(vScroll.ItemList);
    }
}