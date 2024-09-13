using GodotUtilities.Logic;

namespace HexGeneral.Game;

public class PopBuilding : BuildingModel
{
    public float Pop { get; private set; }
    public override void Produce(Location location, ProcedureKey key)
    {
        var data = key.Data.Data();
        var hex = data.Map.Hexes[location.Hex.Coords];
        var regime = hex.Regime.Get(data);
        var perPop = data.Settings.RecruitsPerPop.Value;
        var prod = Pop * perPop;
        regime.AddRecruits(prod, key);
    }
}