using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Logic.Procedure;

namespace HexGeneral.Game.Client.Command;

public class UnitMoveCommand : GodotUtilities.Server.Command
{
    public UnitMoveCommand(ERef<Unit> unit, List<HexRef> path, float cost)
    {
        Unit = unit;
        Path = path;
        Cost = cost;
    }

    public ERef<Unit> Unit { get; private set; }
    public List<HexRef> Path { get; private set; }
    public float Cost { get; private set; }
    public override void Handle(LogicKey key)
    {
        key.SendMessage(new UnitMoveProcedure(Unit, Path, Cost));
    }
}