using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.Graphics;
using GodotUtilities.Logic;
using GodotUtilities.Ui;
using HexGeneral.Client.Ui;
using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Game.Client;

public partial class HexGeneralClient : GameClient
{
    public HexGeneralData Data { get; private set; }
    public Settings ClientSettings { get; private set; }
    public ClientEvents ClientEvents { get; private set; }
    public HexGeneralClient(ILogic logic, HexGeneralData data, 
        Guid playerGuid) : base(logic, playerGuid)
    {
        Data = data;
        ClientEvents = new ClientEvents(this);
        ClientSettings = new Settings("Client Settings");
        TextureManager.Setup("Assets/Textures/",
            new List<string>{".svg", ".png"});
        SceneManager.Setup();
        UiController.ModeOption.AddOption(
            new UnitMode(this, "Unit"), "Unit");
        UiController.ModeOption.AddOption(
            new HexMode(this, "Location"), "Location");
        UiController.ModeOption.AddOption(
            new SupplyMode(this), "Supply");
        UiController.ModeOption.AddOption(
            new DeploymentMode(this), "Deployment");
        UiController.ModeOption.AddOption(
            new MapEditorMode(this), "Map Editor");

        var mapGraphics = new MapGraphics(this);
        AddComponent(mapGraphics);
        mapGraphics.Input.Input += e =>
        {
            UiController.Mode?.HandleInput(e);
        };
        var uiFrame = GetComponent<UiFrame>();
        foreach (var option in UiController.ModeOption.Options)
        {
            uiFrame.LeftBar.Add(() =>
                {
                    UiController.ModeOption.Choose(option);
                },
                option.Name);
        }
        UiController.ModeOption.SettingChanged.SubscribeForNode(
            a =>
            {
                uiFrame.LeftBar.ShowPanel(a.newVal.GetControl(this));
            }, this);
        var topButtons = new HBoxContainer();
        uiFrame.TopBars.AddChild(topButtons);
        topButtons.AddButton("Map Graphics Settings",
            () => GraphicsSettingsWindow.Open(this));
        topButtons.AddButton("Logger",
            () => LoggerWindow.Open(this, Data));
        topButtons.AddButton("Save",
            () => SaverWindow.Open(this));
        topButtons.AddButton("Load",
            () => LoaderWindow.Open(this));
        AddComponent(new TurnBar());
        AddComponent(new RegimeInfoBar());
        AddComponent(new RegimeControlBar());
    }
    public override void _Ready()
    {
        AddComponent(new CameraController());
        
    }

    public Player GetPlayer()
    {
        return Data.PlayerHolder.PlayerByGuid.TryGetValue(PlayerGuid, out var p)
            ? p.Get(Data)
            : null;
    }
    //DON'T REMOVE THIS
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}