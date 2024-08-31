using GodotUtilities.Ui;

namespace HexGeneral.Game;

public class NewGameSettings : Settings
{
    public FloatSettingsOption Height { get; private set; }
    public FloatSettingsOption Width { get; private set; }
    public FloatSettingsOption LandRatio { get; private set; }
    public FloatSettingsOption Roughness { get; private set; }
    public FloatSettingsOption Moisture { get; private set; }
    public FloatSettingsOption NoiseScale { get; private set; }
    public NewGameSettings() : base("New Game")
    {
        Width = new FloatSettingsOption("Width",
            300, 50, 1000, 1, true);

        Height = new FloatSettingsOption("Height",
            200, 50, 1000, 1, true);
        
        LandRatio = new FloatSettingsOption("Land Ratio",
            .4f, .1f, 1f, .05f, false);
        
        Roughness = new FloatSettingsOption("Roughness",
            .5f, 0f, 1f, .05f, false);
        Moisture = new FloatSettingsOption("Moisture",
            .5f, 0f, 1f, .05f, false);
        
        NoiseScale = new FloatSettingsOption("NoiseScale",
            1f, .1f, 10f, .05f, false);
        
        SettingsOptions.Add(Width);
        SettingsOptions.Add(Height);
        SettingsOptions.Add(LandRatio);
        SettingsOptions.Add(Roughness);
        SettingsOptions.Add(Moisture);
        SettingsOptions.Add(NoiseScale);
    }
}