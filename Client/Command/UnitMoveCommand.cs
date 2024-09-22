using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Logic.Procedure;

namespace HexGeneral.Game.Client.Command;

public class UnitMoveCommand : GodotUtilities.Server.Command
{
    public ERef<Unit> Unit { get; private set; }
    public List<HexRef> Path { get; private set; }
    public float MoveRatioCost { get; private set; }
    public UnitMoveCommand(ERef<Unit> unit, List<HexRef> path, 
        float moveRatioCost)
    {
        Unit = unit;
        Path = path;
        MoveRatioCost = moveRatioCost;
    }
    public override void Handle(LogicKey key)
    {
        key.SendMessage(new UnitMoveProcedure(Unit, Path, 
            MoveRatioCost));
    }
}