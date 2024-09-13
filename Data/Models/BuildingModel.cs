using GodotUtilities.GameData;
using GodotUtilities.Logic;

namespace HexGeneral.Game;

public abstract class BuildingModel : Model
{
    public abstract void Produce(Location location, ProcedureKey key);
}