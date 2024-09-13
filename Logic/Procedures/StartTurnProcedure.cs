using System.Linq;
using GodotUtilities.Logic;
using GodotUtilities.Server;

namespace HexGeneral.Game.Procedures;

public class StartTurnProcedure : Procedure
{
    public override void Handle(ProcedureKey key)
    {
        var data = key.Data.Data();
        data.Notices.TurnStarted?.Invoke();
        var turnManager = data.TurnManager;
        var regime = turnManager.GetCurrentRegime().Get(data);
        var locs = data.LocationHolder.Locations
            .Where(kvp => regime.Hexes.Contains(kvp.Key.Coords))
            .Select(kvp => kvp.Value.Get(data));
        foreach (var loc in locs)
        {
            foreach (var modelIdRef in loc.Buildings)
            {
                var building = modelIdRef.Get(data);
                building.Produce(loc, key);
            }
        }
        
        
        foreach (var unit in data.Entities.GetAll<Unit>()
                     .Where(u => u.Regime == regime))
        {
            unit.RefreshForTurn(key);
        }
        
        
        
        
        data.Notices.FinishedTurnStartLogic?.Invoke();
        turnManager.AcceptCommands(key);
    }
}