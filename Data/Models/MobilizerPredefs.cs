namespace HexGeneral.Game;

public class MobilizerPredefs : IPredefHolder<Mobilizer>
{
    private HexGeneralData _data;
    public Mobilizer Transport => _data.Models.GetModel<Mobilizer>(nameof(Transport));

    public MobilizerPredefs(HexGeneralData data)
    {
        _data = data;
    }
}