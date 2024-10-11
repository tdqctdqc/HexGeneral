using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Ui;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Logic.Editor;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Client.Ui;

public class MapEditorMode : UiMode
{
    public MouseMode DrawLandformMode { get; private set; }
    public SettingsOption<Landform> SelectedLandform { get; private set; }
    public MouseMode DrawVegetationMode { get; private set; }
    public SettingsOption<Vegetation> SelectedVegetation { get; private set; }
    public MouseMode DrawRoadMode { get; private set; }
    public SettingsOption<RoadModel> SelectedRoadModel { get; private set; }
    public MouseMode DrawRegimeMode { get; private set; }
    public SettingsOption<Regime> SelectedRegime { get; private set; }
    public FloatSettingsOption BrushSize { get; private set; }
    public Stack<EditorAction> EditorActions { get; private set; }
    public Action DidEditorAction { get; set; }
    public MapEditorMode(HexGeneralClient client) 
        : base(client, "Map Editor")
    {
        EditorActions = new Stack<EditorAction>();
        BrushSize = new FloatSettingsOption("Brush Size",
            1f, 1f, 10f, 1f, true);
        SelectedLandform = new DefaultSettingsOption<Landform>("Landform", null);
        SelectedLandform.Set(client.Data.Models.GetModels<Landform>().First());
        
        SelectedVegetation = new DefaultSettingsOption<Vegetation>("Vegetation", null);
        SelectedVegetation.Set(client.Data.Models.GetModels<Vegetation>().First());
        
        SelectedRegime = new DefaultSettingsOption<Regime>("Regime", null);
        SelectedRegime.Set(client.Data.Entities.GetAll<Regime>().First());
        
        SelectedRoadModel = new DefaultSettingsOption<RoadModel>("Road Model", null);
        SelectedRoadModel.Set(client.Data.Models.GetModels<RoadModel>().First());
        
        DrawLandformMode = new MouseMode([new PaintHexMouseAction<Landform>(MouseButtonMask.Right,
            BrushSize, SelectedLandform, 
            (lf, h) => ChangeTerrainAction.Construct(h, lf, client.Data), 
            client,
            DoEditorAction)]);
        DrawVegetationMode = new MouseMode([new PaintHexMouseAction<Vegetation>(MouseButtonMask.Right,
            BrushSize, SelectedVegetation, 
            (v, h) => ChangeTerrainAction.Construct(h, v, client.Data), 
            client,
            DoEditorAction)]);
        DrawRegimeMode = new MouseMode([new PaintHexMouseAction<Regime>(MouseButtonMask.Right,
            BrushSize, SelectedRegime, 
            (r, h) => ChangeRegimeAction.Construct(h, r, client.Data), 
            client,
            DoEditorAction)]);
        
        MouseMode.Set(DrawLandformMode);
    }

    public void DoEditorAction(EditorAction a)
    {
        EditorActions.Push(a);
        _client.SubmitCommand(
            new DoProcedureCommand(
                new EditorActionProcedure(a)
            )
        );
        DidEditorAction?.Invoke();
    }

    public void UndoEditorAction()
    {
        var a = EditorActions.Pop();
        _client.SubmitCommand(
            new DoProcedureCommand(
                new EditorActionProcedure(a.GetUndoAction())
            )
        );
        DidEditorAction?.Invoke();
    }
    public override void Process(float delta)
    {
        
    }

    public override void HandleInput(InputEvent e)
    {
        if (e is InputEventMouse mb)
        {
            MouseMode.Value?.HandleInput(mb);
        }
    }

    public override void Enter()
    {
    }

    public override void Clear()
    {
        MouseMode.Value?.Clear();
    }

    public override Control GetControl(GameClient client)
    {
        return new MapEditorPanel(this, _client.Client().Data);
    }
}