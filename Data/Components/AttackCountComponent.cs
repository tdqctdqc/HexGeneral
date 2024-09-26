using System;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game.Client;
using HexGeneral.Game.Logic;

namespace HexGeneral.Game.Components;

public class AttackCountComponent : IUnitCombatComponent
{
    public AttackCountComponent(int attacksTaken, int maxAttacks)
    {
        AttacksTaken = attacksTaken;
        MaxAttacks = maxAttacks;
    }

    public int MaxAttacks { get; private set; }
    public int AttacksTaken { get; private set; }
    public Control GetDisplay(GameClient client)
    {
        var l = new Label();
        l.Text = $"Attacks remaining: {MaxAttacks - AttacksTaken} / {MaxAttacks}";
        return l;
    }

    public void TurnTick(ProcedureKey key)
    {
        AttacksTaken = 0;
    }

    public void Added(EntityComponentHolder holder, GodotUtilities.GameData.Data data)
    {
    }

    public void Removed(EntityComponentHolder holder, GodotUtilities.GameData.Data data)
    {
    }

    public void SpendAttack(ProcedureKey key)
    {
        AttacksTaken++;
    }

    public void ModifyAsAttacker(CombatModifier modifier, HexGeneralData data)
    {
    }

    public void ModifyAsDefender(CombatModifier modifier, HexGeneralData data)
    {
    }

    public void AfterCombat(ProcedureKey key)
    {
        
    }

    public bool AttackBlocked(HexGeneralData data)
    {
        return AttacksTaken >= MaxAttacks;
    }

    public bool DefendBlocked(HexGeneralData data)
    {
        return false;
    }
}