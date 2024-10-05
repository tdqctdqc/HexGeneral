using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using GodotUtilities.Server;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Components;
using HexGeneral.Game.Logic;

namespace HexGeneral.Data.Components;

public class EmbarkedComponent(ERef<Unit> unit, IMoveComponent inner, ModelIdRef<Mobilizer> mobilizer)
    : IMoveComponent
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public IMoveComponent Inner { get; private set; } = inner;
    public ModelIdRef<Mobilizer> Mobilizer { get; private set; } = mobilizer;

    public Control GetDisplay(GameClient client)
    {
        return new Control();
    }

    public void TurnTick(ProcedureKey key)
    {
    }

    private void SwitchDomainIfNeeded(bool entering, HexGeneralData data)
    {
        var (nativeDomain, mobDomain)
            = (Inner
                    .GetActiveMoveType(data).Domain, 
                Mobilizer.Get(data).MoveType.Domain);
        if (nativeDomain == mobDomain) return;
        var (togglingFromDomain, togglingToDomain) = entering 
            ? (nativeDomain, mobDomain)
            : (mobDomain, nativeDomain);
        data.MapUnitHolder.SwitchUnitDomain(Unit.Get(data),
            togglingFromDomain,
            togglingToDomain, data);
    }
    public void Added(EntityComponentHolder holder, 
        GodotUtilities.GameData.Data data)
    {
        var unit = Unit.Get(data);
        if (CanAddRightNow(unit, data.Data()) == false)
        {
            throw new Exception();
        }
        Inner = unit.Components.Get<IMoveComponent>(data);
        holder.Remove(Inner, data);
        SwitchDomainIfNeeded(true, data.Data());
        data.Data().Notices.UnitAltered?.Invoke(unit);
    }

    public void Removed(EntityComponentHolder holder, GodotUtilities.GameData.Data data)
    {
        var unit = Unit.Get(data);
        unit.Components.Add(Inner, data);
        SwitchDomainIfNeeded(false, data.Data());
        data.Data().Notices.UnitAltered?.Invoke(unit);
    }

    public void ModifyAsAttacker(CombatModifier modifier, HexGeneralData data)
    {
        Mobilizer.Get(data).ModifyAsAttacker(modifier, data);
    }

    public void ModifyAsDefender(CombatModifier modifier, HexGeneralData data)
    {
        Mobilizer.Get(data).ModifyAsDefender(modifier, data);
    }

    public void AfterCombat(ProcedureKey key)
    {
        
    }

    public bool AttackBlocked(HexGeneralData data)
    {
        return Mobilizer.Get(data).CanAttack == false;
    }

    public bool DefendBlocked(HexGeneralData data)
    {
        return Mobilizer.Get(data).CanDefend == false;
    }

    public HashSet<Hex> GetMoveRadius(Unit unit, HexGeneralData data)
    {
        return Mobilizer.Get(data).MoveType.GetMoveRadius(unit, data);
    }

    public void DrawRadius(Unit unit, 
        MapOverlayDrawer mesh, HexGeneralData data)
    {
        GetActiveMoveType(data).DrawRadius(unit, mesh, data);
    }

    public void TryMoveCommand(Unit unit, Hex dest, Action<Command> submit, HexGeneralClient client)
    {
        var mob = Mobilizer.Get(client.Data);
        mob.MoveType.TryMoveCommand(unit, mob.MovePoints, dest, 
            submit, client);
    }

    public float GetMovePoints(HexGeneralData data)
    {
        return Mobilizer.Get(data).MovePoints;
    }

    public MoveType GetActiveMoveType(HexGeneralData data)
    {
        return Mobilizer.Get(data).MoveType;
    }
    
    public static bool CanAddRightNow(Unit u, HexGeneralData data)
    {
        return u.Components.Get<MoveCountComponent>(data).HasMoved() == false;
    }
}