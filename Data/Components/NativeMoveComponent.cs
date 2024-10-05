using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using GodotUtilities.Logic;
using GodotUtilities.Server;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Command;
using HexGeneral.Game.Logic;
using HexGeneral.Logic.Procedure;

namespace HexGeneral.Game.Components;

public class NativeMoveComponent(ERef<Unit> unit) 
    : INativeMoveComponent
{
    public ERef<Unit> Unit { get; private set; } = unit;

    public Control GetDisplay(GameClient client)
    {
        var l = new Label();
        var data = client.Client().Data;
        var unit = Unit.Get(data);
        var model = unit.UnitModel.Get(data);
        
        var moveRatio = unit.Components.Get<MoveCountComponent>(data).MovePointRatioRemaining;

        l.Text = $"Move Points: {model.MovePoints * moveRatio} / {model.MovePoints}";

        return l;
    }

    public void TurnTick(ProcedureKey key)
    {
        
    }

    public void Added(EntityComponentHolder holder, GodotUtilities.GameData.Data data)
    {
    }

    public void Removed(EntityComponentHolder holder, GodotUtilities.GameData.Data data)
    {
    }
    public void ModifyAsAttacker(CombatModifier modifier, HexGeneralData data)
    {
        Unit.Get(data).UnitModel.Get(data)
            .MoveType.ModifyAsAttacker(modifier, data);
    }

    public void ModifyAsDefender(CombatModifier modifier, HexGeneralData data)
    {
        Unit.Get(data).UnitModel.Get(data)
            .MoveType.ModifyAsDefender(modifier, data);
    }

    public void AfterCombat(ProcedureKey key)
    {
        
    }

    public HashSet<Hex> GetMoveRadius(Unit unit, HexGeneralData data)
    {
        var moveCount = unit.Components.Get<MoveCountComponent>(data);
        if (moveCount.CanMove() == false) return new HashSet<Hex>();
        var hex = unit.GetHex(data);
        var model = unit.UnitModel.Get(data);
        var regime = unit.Regime.Get(data);
        var moveRatio = moveCount.MovePointRatioRemaining;
        return model.MoveType.GetMoveRadius(hex, regime, 
            model.MovePoints * moveRatio,
                data);
    }

    public void DrawRadius(Unit unit, 
        MapOverlayDrawer mesh, HexGeneralData data)
    {
        GetActiveMoveType(data).DrawRadius(unit, mesh, data);
    }

    
    
    public void TryMoveCommand(Unit unit, Hex dest, 
        Action<Command> submit, HexGeneralClient client)
    {
        var model = unit.UnitModel.Get(client.Data);
        model.MoveType.TryMoveCommand(unit, model.MovePoints, dest,
            submit, client);
    }
    
    public float GetMovePoints(HexGeneralData data)
    {
        return Unit.Get(data).UnitModel.Get(data).MovePoints;
    }
    
    public MoveType GetActiveMoveType(HexGeneralData data)
    {
        return Unit.Get(data).UnitModel.Get(data).MoveType;
    }

    public bool AttackBlocked(HexGeneralData data)
    {
        return false;
    }

    public bool DefendBlocked(HexGeneralData data)
    {
        return false;
    }
}