using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Ui;
using HexGeneral.Game;
using HexGeneral.Game.Client;

namespace HexGeneral.Client.Ui;

public partial class NewGameIntroPanel : PanelContainer
{
    private HexGeneralData _data;

    public NewGameIntroPanel(HexGeneralData data)
    {
        _data = data;
    }

    public override void _Ready()
    {
        Size = new Vector2(200f, 200f);
        var vbox = new VBoxContainer();
        vbox.AddButton("Start Game",
            () =>
            {
                var client = new HexGeneralClient(_data);
                var parent = GetParent();
                parent.QueueFree();
                this.QueueFree();
                Root.I.AddChild(client);
                Root.I.SetClient(client);
            });
        AddChild(vbox);
        this.Center();
    }
}