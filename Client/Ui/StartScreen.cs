using Godot;

namespace HexGeneral.Client.Ui;
public partial class StartScreen : CanvasLayer
{
    public override void _Ready()
    {
        var menu = new MainMenu();
        AddChild(menu);
    }
}