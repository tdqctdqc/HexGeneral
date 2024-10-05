using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.PathFinder;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using GodotUtilities.Logic;
using GodotUtilities.Server;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Command;
using HexGeneral.Game.Components;
using HexGeneral.Game.Logic;

namespace HexGeneral.Game;

public abstract class MoveType : Model
{
    public Domain Domain { get; private set; }

    public abstract float GetMoveCost(Hex from, Hex to,
        Regime movingRegime, HexGeneralData data);
    
    public List<Hex> GetPath(Hex start, Hex end, 
        Regime movingRegime, HexGeneralData data,
        out float cost)
    {
        if (end.Full(Domain, data))
        {
            cost = Single.PositiveInfinity;
            return null;
        }
        var path = PathFinder<Hex>.FindPathAStar(
            start, end, h => h.GetNeighbors(data),
            (h, g) => GetMoveCost(h, g, movingRegime, data),
            (h, g) => GetMoveCost(h, g, movingRegime, data), out var pCost);
        cost = pCost;
        return path;
    }

    public HashSet<Hex> GetMoveRadius(Unit unit, HexGeneralData data)
    {
        var moveCount = unit.Components.Get<MoveCountComponent>(data);
        if (moveCount.CanMove() == false) return new HashSet<Hex>();
        var ratio = moveCount.MovePointRatioRemaining;
        var hex = unit.GetHex(data);
        var model = unit.UnitModel.Get(data);
        var regime = unit.Regime.Get(data);
        return GetMoveRadius(hex, regime, model.MovePoints * ratio,
                data);
    }
    public abstract HashSet<Hex> GetMoveRadius(Hex start, 
        Regime movingRegime,
        float maxCost, HexGeneralData data);
    public abstract void MoveThru(Hex hex, Unit unit, ProcedureKey key);
    public abstract void ModifyAsAttacker(CombatModifier modifier, HexGeneralData data);
    public abstract void ModifyAsDefender(CombatModifier modifier, HexGeneralData data);
    public abstract bool ValidDest(Hex hex, Regime movingRegime, HexGeneralData data);

    
    public void TryMoveCommand(Unit unit, float movePoints, 
        Hex dest, Action<Command> submit, HexGeneralClient client)
    {
        var moveCount = unit.Components.Get<MoveCountComponent>(client.Data);
        if (moveCount.CanMove() == false)
        {
            return;
        }
        if (ValidDest(dest, unit.Regime.Get(client.Data), client.Data)
            == false)
        {
            return;
        }
        var path = GetPath(unit.GetHex(client.Data), dest, unit.Regime.Get(client.Data),
                client.Data, out var c);
        var costRatio = c / movePoints;

        if (path is null || costRatio > moveCount.MovePointRatioRemaining)
        {
            return;
        }

        submit(new UnitMoveCommand(unit.MakeRef(), 
            path.Select(h => h.MakeRef()).ToList(), costRatio));
    }
    
    public void DrawPath(Unit unit, Hex dest, 
        MapOverlayDrawer mesh, HexGeneralData data)
    {
        
        var path = GetPath(unit.GetHex(data), dest, unit.Regime.Get(data),
                data, out var cost);
        if (path is null) return;
        mesh.Draw(mb =>
        {
            for (var i = 0; i < path.Count - 1; i++)
            {
                var from = path[i];
                var to = path[i + 1];
                mb.AddArrow(from.WorldPos(), to.WorldPos(), .25f, Colors.Black);
                mb.AddArrow(from.WorldPos(), to.WorldPos(), .2f, Colors.White);
            }
        }, Vector2.Zero);
        var tt = SceneManager.Instance<MoveTooltip>();

        tt.DrawInfo(unit, cost, data);
        mesh.AddNode(tt, dest.WorldPos());
    }
    public void DrawRadius(Unit unit, 
        MapOverlayDrawer mesh, HexGeneralData data)
    {
        var radius = GetMoveRadius(unit, data);
        mesh.Draw(mb =>
        {
            foreach (var h in radius)
            {
                mb.DrawHex(h.WorldPos(), 1f, 
                    Colors.White.Tint(.5f));
            }

        }, Vector2.Zero);
    }
    
}