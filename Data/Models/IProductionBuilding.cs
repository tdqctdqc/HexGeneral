using GodotUtilities.Logic;

namespace HexGeneral.Game;

public interface IProductionBuilding
{
    void Produce(Location location, ProcedureKey key);
}