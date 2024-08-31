using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class Regime : Entity
{
    public ModelIdRef<RegimeModel> RegimeModel { get; private set; }
    public Regime(int id, ModelIdRef<RegimeModel> regimeModel) : base(id)
    {
        RegimeModel = regimeModel;
    }

    public override void Made(Data d)
    {
    }

    public override void CleanUp(Data d)
    {
    }
}