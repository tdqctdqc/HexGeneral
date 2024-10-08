namespace HexGeneral.Game;

public class ModelPredefs
{
    public LandformPredefs Landforms { get; private set; }
    public VegetationPredefs Vegetations { get; private set; }
    public RoadModelPredefs RoadModels { get; private set; }
    public UnitModelPredefs UnitModelPredefs { get; private set; }
    public MoveTypePredefs MoveTypes { get; private set; }
    public DomainPredefs Domains { get; private set; }
    public BuildingPredefs Buildings { get; private set; }
    public MobilizerPredefs Mobilizers { get; private set; }
    public ModelPredefs(HexGeneralData data)
    {
        Vegetations = new VegetationPredefs(data);
        Landforms = new LandformPredefs(data);
        RoadModels = new RoadModelPredefs(data);
        UnitModelPredefs = new UnitModelPredefs(data);
        MoveTypes = new MoveTypePredefs(data);
        Domains = new DomainPredefs(data);
        Buildings = new BuildingPredefs(data);
        Mobilizers = new MobilizerPredefs(data);
    }
}