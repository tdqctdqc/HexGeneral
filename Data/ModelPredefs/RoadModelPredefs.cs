namespace HexGeneral.Game;

public class RoadModelPredefs : IPredefHolder<RoadModel>

{
    public RoadModel Dirt => _data.Models.GetModel<RoadModel>(nameof(Dirt));

    private HexGeneralData _data;

    public RoadModelPredefs(HexGeneralData data)
    {
        _data = data;
    }
}