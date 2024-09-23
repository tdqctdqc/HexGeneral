using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using GodotUtilities.Server;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Command;
using HexGeneral.Game.Logic;
using HexGeneral.Logic.Procedure;

namespace HexGeneral.Game.Components;

public class NativeMoveComponent(ERef<Unit> unit) : INativeMoveComponent
{
    public ERef<Unit> Unit { get; private set; } = unit;

    public Control GetDisplay(HexGeneralClient client)
    {
        var l = new Label();
        var unit = Unit.Get(client.Data);
        var model = unit.UnitModel.Get(client.Data);
        
        var moveRatio = unit.Components.Get<MoveCountComponent>().MovePointRatioRemaining;

        l.Text = $"Move Points: {model.MovePoints * moveRatio} / {model.MovePoints}";

        return l;
    }

    public void TurnTick(ProcedureKey key)
    {
    }

    public void Added(ProcedureKey key)
    {
        
    }

    public void Removed(ProcedureKey key)
    {
        
    }

    public void Modify(CombatModifier modifier, HexGeneralData data)
    {
        var effect = modifier.Unit.UnitModel.Get(data)
            .MoveType
            .GetDamageMult(modifier.Hex, data,
                modifier.OnOffense);
        modifier.AddDamageTakenMult(effect, "Native Move Type Terrain Effect");
    }

    public HashSet<Hex> GetMoveRadius(Unit unit, HexGeneralData data)
    {
        var moveCount = unit.Components.Get<MoveCountComponent>();
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
        var radius = GetMoveRadius(unit, data);
        var hexTris = HexExt.UnitHexTris;
        mesh.Draw(mb =>
        {
            foreach (var h in radius)
            {
                mb.AddTris(hexTris
                        .Select(p => p + h.WorldPos()), 
                    Colors.White.Tint(.5f));
            }

        }, Vector2.Zero);
    }

    public void DrawPath(Unit unit, Hex dest, MapOverlayDrawer mesh, HexGeneralData data)
    {
        var path = unit.UnitModel.Get(data).MoveType
            .GetPath(unit.GetHex(data), dest, unit.Regime.Get(data),
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
    
    public Command GetMoveCommand(Unit unit, Hex dest, HexGeneralClient client)
    {
        var moveCountComponent = unit.Components.Get<MoveCountComponent>();
        if (moveCountComponent.CanMove() == false) return null;
        
        var model = unit.UnitModel.Get(client.Data);
        var path = model.MoveType
            .GetPath(unit.GetHex(client.Data), dest, unit.Regime.Get(client.Data),
                client.Data, out var c);

        var costRatio = c / model.MovePoints;
        
        if (path is null 
            || costRatio > moveCountComponent.MovePointRatioRemaining)
        {
            return null;
        }

        return new UnitMoveCommand(unit.MakeRef(), 
            path.Select(h => h.MakeRef()).ToList(), costRatio);
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
}