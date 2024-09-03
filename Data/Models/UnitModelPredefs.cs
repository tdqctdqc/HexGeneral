namespace HexGeneral.Game;

public class UnitModelPredefs : IPredefHolder<UnitModel>
{
    public UnitModel Infantry => _data.Models.GetModel<UnitModel>(nameof(Infantry));
    private HexGeneralData _data;

    public UnitModelPredefs(HexGeneralData data)
    {
        _data = data;
    }
}