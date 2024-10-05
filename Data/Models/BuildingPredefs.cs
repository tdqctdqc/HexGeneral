namespace HexGeneral.Game;

public class BuildingPredefs(HexGeneralData data) : IPredefHolder<BuildingModel>
{
    private HexGeneralData _data = data;

    public PopBuilding Village
        => (PopBuilding)_data.Models.ModelsByName[nameof(Village)];
    public PopBuilding Town
        => (PopBuilding)_data.Models.ModelsByName[nameof(Town)];
    public PopBuilding City
        => (PopBuilding)_data.Models.ModelsByName[nameof(City)];
    public PopBuilding Metropolis
        => (PopBuilding)_data.Models.ModelsByName[nameof(Metropolis)];
    public IndustryBuilding Factory
        => (IndustryBuilding)_data.Models.ModelsByName[nameof(Factory)];
    public AirbaseBuilding Airbase
        => (AirbaseBuilding)_data.Models.ModelsByName[nameof(Airbase)];
    public PortBuilding Port
        => (PortBuilding)_data.Models.ModelsByName[nameof(Port)];

}