using System.Collections.Generic;
using GodotUtilities.GameClient;
using GodotUtilities.Graphics;
using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Game.Client;

public partial class HexGeneralClient : GameClient
{

    public HexGeneralClient()
    {
        TextureManager.Setup("Assets/Textures/",
            new List<string>{".svg", ".png"});
    }
    public void OpenGameSession(HexGeneralData data)
    {
        AddComponent(new CameraController());
        AddComponent(new MapGraphics(data));
        UiController.ModeOption.AddOption(
            new UnitMode(this, "Unit"), "Unit");
        UiController.ModeOption.Choose<UnitMode>();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}