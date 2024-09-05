namespace HexGeneral.Game;

public class PopBuilding : BuildingModel
{
    public float Pop { get; private set; }
    public override void Produce(Location location, HexGeneralData data)
    {
        var hex = data.Map.Hexes[location.Coords];
        var regime = hex.Regime.Get(data);
        var perPop = data.Settings.RecruitsPerPop.Value;
        var prod = Pop * perPop;
        regime.AddRecruits(prod);
    }
}