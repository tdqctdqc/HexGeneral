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

        
        AddComponent(new MapGraphics(this));
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
        var settingsButtons = new HBoxContainer();
        uiFrame.TopBars.AddChild(settingsButtons);
        settingsButtons.AddButton("Map Graphics Settings",
            () => GraphicsSettingsWindow.Open(this));
        AddComponent(new TurnBar());
        AddComponent(new RegimeInfoBar());
    }
    public override void _Ready()
    {
        AddComponent(new CameraController());
        
    }

    //DON'T REMOVE THIS
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}