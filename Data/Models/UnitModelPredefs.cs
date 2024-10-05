namespace HexGeneral.Game;

public class UnitModelPredefs : IPredefHolder<UnitModel>
{
    public UnitModel Infantry => _data.Models.GetModel<UnitModel>(nameof(Infantry));
    public UnitModel Engineer => _data.Models.GetModel<UnitModel>(nameof(Engineer));
    private HexGeneralData _data;

    public UnitModelPredefs(HexGeneralData data)
    {
        _data = data;
    }
}