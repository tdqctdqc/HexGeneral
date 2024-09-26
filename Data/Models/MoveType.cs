using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.PathFinder;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
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

    public abstract HashSet<Hex> GetMoveRadius(Hex start, Regime movingRegime,
        float maxCost, HexGeneralData data);
    public abstract void MoveThru(Hex hex, Unit unit, ProcedureKey key);
    public abstract IMoveComponent MakeNativeMoveComponent(ERef<Unit> unit);
    public abstract void ModifyAsAttacker(CombatModifier modifier, HexGeneralData data);
    public abstract void ModifyAsDefender(CombatModifier modifier, HexGeneralData data);
}