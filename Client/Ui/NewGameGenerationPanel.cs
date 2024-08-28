using Godot;
using GodotUtilities.Ui;
using HexGeneral.Game;
using HexGeneral.Game.Generators;

namespace HexGeneral.Client.Ui;

public partial class NewGameGenerationPanel : PanelContainer
{
    public override void _Ready()
    {
        Size = new Vector2(200f, 200f);
        var vbox = new VBoxContainer();

        var settings = new NewGameSettings();
        vbox.AddChild(settings.GetControlInterface());
        
        vbox.AddButton("Make",
            () =>
            {
                var data = NewGameGenerator.Generate(settings);
                GetParent().AddChild(new NewGameIntroPanel(data));
                this.QueueFree();
            });
        AddChild(vbox);
        this.Center();
    }
}