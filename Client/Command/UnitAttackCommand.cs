using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game.Logic;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game.Client.Command;

public class UnitAttackCommand(ERef<Unit> unit, Vector3I target, Vector3I from) : GodotUtilities.Server.Command
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public Vector3I Target { get; private set; } = target;
    public Vector3I From { get; private set; } = from;

    public override void Handle(LogicKey key)
    {
        CombatLogic.HandleAttack(Unit.Get(key.Data),
            key.Data.Data().Map.Hexes[Target], key);
    }
}