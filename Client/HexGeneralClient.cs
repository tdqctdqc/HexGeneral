using System.Collections.Generic;
using System.Linq;
using GodotUtilities.GameClient;
using GodotUtilities.Graphics;
using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Game.Client;

public partial class HexGeneralClient : GameClient
{
    public HexGeneralData Data { get; private set; }
    public HexGeneralClient(HexGeneralData data)
    {
        Data = data;
        TextureManager.Setup("Assets/Textures/",
            new List<string>{".svg", ".png"});
        UiController.ModeOption.AddOption(
            new UnitMode(this, "Unit"), "Unit");
        UiController.ModeOption.AddOption(
            new HexMode(this, "Location"), "Location");
    }
    public override void _Ready()
    {
        AddComponent(new CameraController());
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
    }

    //DON'T REMOVE THIS
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}