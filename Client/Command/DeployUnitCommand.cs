using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game.Client.Command;

public class DeployUnitCommand : GodotUtilities.Server.Command
{
    public ERef<Unit> Unit { get; private set; }
    public HexRef Hex { get; private set; }

    public DeployUnitCommand(ERef<Unit> unit, HexRef hex)
    {
        Unit = unit;
        Hex = hex;
    }

    public override void Handle(LogicKey key)
    {
        var proc = new DeployUnitProcedure(Unit, Hex);
        key.SendMessage(proc);
    }
}