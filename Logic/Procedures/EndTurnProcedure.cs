using GodotUtilities.Logic;
using GodotUtilities.Server;

namespace HexGeneral.Game.Procedures;

public class EndTurnProcedure : Procedure
{
    public override void Handle(ProcedureKey key)
    {
        var turnManager = key.Data.Data().TurnManager;
        turnManager.RejectCommands(key);
        turnManager.Iterate(key);
        new StartTurnProcedure().Handle(key);
    }
}