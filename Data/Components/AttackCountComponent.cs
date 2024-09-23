using Godot;
using GodotUtilities.Logic;
using HexGeneral.Game.Client;

namespace HexGeneral.Game.Components;

public class AttackCountComponent : IEntityComponent
{
    public AttackCountComponent(int attacksTaken, int maxAttacks)
    {
        AttacksTaken = attacksTaken;
        MaxAttacks = maxAttacks;
    }

    public int MaxAttacks { get; private set; }
    public int AttacksTaken { get; private set; }
    public Control GetDisplay(HexGeneralClient client)
    {
        var l = new Label();
        l.Text = $"Attacks remaining: {MaxAttacks - AttacksTaken} / {MaxAttacks}";
        return l;
    }

    public void TurnTick(ProcedureKey key)
    {
        AttacksTaken = 0;
    }

    public void Added(ProcedureKey key)
    {
        
    }

    public void Removed(ProcedureKey key)
    {
        
    }

    public void SpendAttack(ProcedureKey key)
    {
        AttacksTaken++;
    }

    public bool CanAttack(Unit unit, HexGeneralData data)
    {
        if (unit.Components.Get<IMoveComponent>().AttackBlocked(data))
        {
            return false;
        }
        return AttacksTaken < MaxAttacks;
    }
}