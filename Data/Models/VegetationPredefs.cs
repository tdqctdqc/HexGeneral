namespace HexGeneral.Game;

public class VegetationPredefs : IPredefHolder<Vegetation>
{
    public Vegetation Barren 
        => _data.Models.GetModel<Vegetation>(nameof(Barren));
    public Vegetation Forest 
        => _data.Models.GetModel<Vegetation>(nameof(Forest));
    public Vegetation Grassland 
        => _data.Models.GetModel<Vegetation>(nameof(Grassland));
    private HexGeneralData _data;

    public VegetationPredefs(HexGeneralData data)
    {
        _data = data;
    }
}