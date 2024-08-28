using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Ui;
using HexGeneral.Game;

namespace HexGeneral.Client.Ui;

public partial class NewGameIntroPanel : PanelContainer
{
    private Data _data;

    public NewGameIntroPanel(Data data)
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
                var client = new GameClient();
                var parent = GetParent();
                parent.QueueFree();
                this.QueueFree();
                Root.I.AddChild(client);
            });
        AddChild(vbox);
        this.Center();
    }
}