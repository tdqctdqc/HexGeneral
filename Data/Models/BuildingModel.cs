using GodotUtilities.GameData;

namespace HexGeneral.Game;

public abstract class BuildingModel : Model
{
    public abstract void Produce(Location location, HexGeneralData data);
}