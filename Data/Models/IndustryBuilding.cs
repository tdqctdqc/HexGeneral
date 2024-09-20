using GodotUtilities.GameData;
using GodotUtilities.Logic;

namespace HexGeneral.Game;

public class IndustryBuilding : BuildingModel
{
    public float IndustrialProd { get; private set; }
    public override void Produce(Location location, ProcedureKey key)
    {
        var regime = location.Hex.Get(key.Data).Regime.Get(key.Data);
        regime.IncrementIndustrialPoints(IndustrialProd, key);
    }
}