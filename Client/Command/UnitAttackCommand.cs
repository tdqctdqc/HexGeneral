using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game.Logic;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game.Client.Command;

public class UnitAttackCommand(ERef<Unit> unit, ERef<Unit> target) : GodotUtilities.Server.Command
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public ERef<Unit> Target { get; private set; } = target;

    public override void Handle(LogicKey key)
    {
        CombatLogic.HandleAttack(Unit.Get(key.Data),
            Target.Get(key.Data), key);
    }
}