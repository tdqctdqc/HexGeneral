using GodotUtilities.Logic;
using HexGeneral.Game.Procedures;

namespace HexGeneral.Game.Client.Command;

public class SubmitTurnCommand : GodotUtilities.Server.Command
{
    public override void Handle(LogicKey key)
    {
        var data = key.Data.Data();
        var player = this.GetCommandingPlayer(data);
        var currRegime = data.TurnManager.GetCurrentRegime();
        if (player.Regime == currRegime)
        {
            key.SendMessage(new EndTurnProcedure());
        }
    }
}