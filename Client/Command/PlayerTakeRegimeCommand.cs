using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game.Procedures;

namespace HexGeneral.Game.Client.Command;

public class PlayerTakeRegimeCommand(ERef<Regime> toTake) : GodotUtilities.Server.Command
{
    public ERef<Regime> ToTake { get; private set; } = toTake;


    public override void Handle(LogicKey key)
    {
        var proc = new SetPlayerRegimeProcedure(CommandingPlayerGuid, ToTake);
        key.SendMessage(proc);
    }
}