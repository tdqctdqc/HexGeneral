using GodotUtilities.Ui;

namespace HexGeneral.Game;

public class GameSettings : Settings
{
    public FloatSettingsOption RecruitsPerPop { get; private set; }
    public FloatSettingsOption IndustrialProdMult { get; private set; }
    public GameSettings() : base("Game Settings")
    {
        RecruitsPerPop = new FloatSettingsOption("Recruits Per Pop",
            .01f, .001f, 1f, .001f, false);
        IndustrialProdMult = new FloatSettingsOption("Industrial Prod Mult",
            1f, .1f, 10f, .1f, false);
        
        SettingsOptions.Add(RecruitsPerPop);
        SettingsOptions.Add(IndustrialProdMult);
    }
}