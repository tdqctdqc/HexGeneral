namespace HexGeneral.Game;

public class LandformPredefs : IPredefHolder<Landform>
{
    public Landform Plain => _data.Models.GetModel<Landform>(nameof(Plain));
    public Landform Hill => _data.Models.GetModel<Landform>(nameof(Hill));
    public Landform Mountain => _data.Models.GetModel<Landform>(nameof(Mountain));
    public Landform Sea => _data.Models.GetModel<Landform>(nameof(Sea));
    public Landform Urban => _data.Models.GetModel<Landform>(nameof(Urban));
    
    private HexGeneralData _data;

    public LandformPredefs(HexGeneralData data)
    {
        _data = data;
    }
}