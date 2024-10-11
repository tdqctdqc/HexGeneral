using Godot;
using GodotUtilities.Ui;
using HexGeneral.Game;

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
        vbox.AddButton("Load",
            () =>
            {
                var l = new LoaderWindow();
                Root.I.AddChild(l);
                l.Size = Vector2I.One * 700; 
                l.PopupCentered();
            });
        AddChild(vbox);
        this.Center();
    }
}