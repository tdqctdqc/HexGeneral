using GodotUtilities.Ui;

namespace HexGeneral.Game;

public class NewGameSettings : Settings
{
    public FloatSettingsOption Height { get; private set; }
    public FloatSettingsOption Width { get; private set; }

    public NewGameSettings() : base("New Game")
    {
        Width = new FloatSettingsOption("Width",
            100, 10, 1000, 1, true);

        Height = new FloatSettingsOption("Height",
            100, 10, 1000, 1, true);
        
        SettingsOptions.Add(Width);
        SettingsOptions.Add(Height);
    }
}