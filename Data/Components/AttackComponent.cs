using System;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using GodotUtilities.Server;
using HexGeneral.Game.Client;
using HexGeneral.Game.Components;
using HexGeneral.Game.Logic;

namespace HexGeneral.Game;

public abstract class AttackComponent 
    : IModelComponent, IUnitCombatComponent
{
    public abstract bool CanAttack(Unit targetUnit, Hex targetHex, HexGeneralData data);
    public abstract Control GetDisplay(GameClient client);
    public abstract void TurnTick(ProcedureKey key);

    public abstract void Added(EntityComponentHolder holder, GodotUtilities.GameData.Data data);

    public abstract void Removed(EntityComponentHolder holder, GodotUtilities.GameData.Data data);

    public abstract void ModifyAsAttacker(CombatModifier modifier, HexGeneralData data);
    public abstract void ModifyAsDefender(CombatModifier modifier, HexGeneralData data);
    public abstract void AfterCombat(ProcedureKey key);
    public abstract bool AttackBlocked(HexGeneralData data);
    public abstract bool DefendBlocked(HexGeneralData data);
}