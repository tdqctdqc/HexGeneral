using Godot;
using GodotUtilities.Ui;
using HexGeneral.Game;
using HexGeneral.Game.Client;

namespace HexGeneral.Client.Ui;

public partial class LoaderWindow : FileDialog
{
    public static LoaderWindow Open(HexGeneralClient client)
    {
        var w = new LoaderWindow();
        client.WindowHolder.OpenWindowFullSize(w);
        return w;
    }
    public LoaderWindow()
    {
        this.MakeFreeable();
        FileMode = FileModeEnum.OpenFile;
        Confirmed += () =>
        {
            Load(CurrentFile, CurrentDir);
        };
    }
    private void Load(string fileName, string dir)
    {
        var (save, logic, guid) = SaveFile.Load(dir, fileName);
        if (save is not null)
        {
            var newClient = new HexGeneralClient(logic, save, guid);
            Root.I.SetClient(newClient);
            QueueFree();
        }
        
        
    }
}