namespace HexGeneral.Game;

public class LandformPredefs : IPredefHolder<Landform>
{
    public Landform Plain => _data.Models.GetModel<Landform>(nameof(Plain));
    public Landform Sea => _data.Models.GetModel<Landform>(nameof(Sea));
    public Landform Urban => _data.Models.GetModel<Landform>(nameof(Urban));
    
    private HexGeneralData _data;

    public LandformPredefs(HexGeneralData data)
    {
        _data = data;
    }
}