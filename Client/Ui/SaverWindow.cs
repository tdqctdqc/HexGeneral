using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.Ui;
using HexGeneral.Game;
using HexGeneral.Game.Client;

namespace HexGeneral.Client.Ui;

public partial class SaverWindow : FileDialog
{
    public static SaverWindow Open(HexGeneralClient client)
    {
        var w = new SaverWindow(client);
        client.WindowHolder.OpenWindowFullSize(w);
        return w;
    }

    private SaverWindow(HexGeneralClient client)
    {
        this.MakeFreeable();
        FileMode = FileModeEnum.SaveFile;
        Confirmed += () =>
        {
            Save(CurrentFile, CurrentDir, client);
        };
    }

    private void Save(string fileName, string dir, HexGeneralClient client)
    {
        SaveFile.Save(dir, fileName, client);
    }
}