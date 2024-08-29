namespace HexGeneral.Game;

public class ModelPredefs
{
    public LandformPredefs Landforms { get; private set; }

    public VegetationPredefs Vegetations { get; private set; }

    public ModelPredefs(HexGeneralData data)
    {
        Vegetations = new VegetationPredefs(data);
        Landforms = new LandformPredefs(data);
    }
}