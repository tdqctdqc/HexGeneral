using Godot;
using GodotUtilities.Ui;
using HexGeneral.Game.Client;

namespace HexGeneral.Client.Ui;

public partial class GraphicsSettingsWindow : Window
{
    private GraphicsSettingsWindow()
    {
        this.MakeFreeable();
    }

    public static GraphicsSettingsWindow Open(HexGeneralClient client)
    {
        var w = new GraphicsSettingsWindow();
        var c = client.ClientSettings.GetControlInterface();
        w.AddChild(c);
        client.WindowHolder.OpenWindowFullSize(w);
        return w;
    }
    
}