using Godot;
using GodotUtilities.Ui;

namespace HexGeneral.Client.Ui;

public partial class MainMenu : PanelContainer
{
    public override void _Ready()
    {
        Size = new Vector2(200f, 200f);
        var vbox = new VBoxContainer();
        vbox.AddButton("New Game",
            () =>
            {
                var newGame = new NewGameGenerationPanel();
                GetParent().AddChild(newGame);
                this.QueueFree();
            });
        AddChild(vbox);
        this.Center();
    }
}