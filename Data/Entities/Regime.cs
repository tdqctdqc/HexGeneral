using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class Regime(int id, ModelIdRef<RegimeModel> regimeModel, float recruits, float industrialPoints) : Entity(id)
{
    public ModelIdRef<RegimeModel> RegimeModel { get; private set; } = regimeModel;
    public float Recruits { get; private set; } = recruits;
    public float IndustrialPoints { get; private set; } = industrialPoints;
    
    public override void Made(Data d)
    {
    }

    public override void CleanUp(Data d)
    {
    }
}