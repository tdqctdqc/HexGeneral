using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.PathFinder;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class MoveType : Model
{
    public static float HostileHexCostMult { get; private set; } = 1.5f;
    public Dictionary<Landform, float> LandformCosts { get; private set; }
    public Dictionary<Vegetation, float> VegetationCosts { get; private set; }
    public Dictionary<RoadModel, float> RoadCosts { get; private set; }
    public float GetMoveCost(Hex from, Hex to, 
        Regime movingRegime, HexGeneralData data)
    {
        var mult = 1f;
        if (to.Regime != movingRegime)
        {
            if(to.GetUnitRefs(data).Any()) return Single.PositiveInfinity;
            mult = HostileHexCostMult;
        }
        var lfCost = LandformCosts[to.Landform.Get(data)];
        if(lfCost < 0) lfCost = Single.PositiveInfinity;
        var vCost = VegetationCosts[to.Vegetation.Get(data)];
        if(vCost < 0) vCost = Single.PositiveInfinity;
        var cost = lfCost + vCost;
        if (data.RoadNetwork.Roads.TryGetValue(from.GetIdEdgeKey(to), out var road))
        {
            cost = Mathf.Min(cost, RoadCosts[road.Get(data)]);
        }
        return cost * mult;
    }

    public List<Hex> GetPath(Hex start, Hex end, Regime movingRegime, HexGeneralData data,
        out float cost)
    {
        if (end.Full(data))
        {
            cost = Single.PositiveInfinity;
            return null;
        }
        var path = PathFinder<Hex>.FindPathAStar(
            start, end, h => h.GetNeighbors(data),
            (h, g) => GetMoveCost(h, g, movingRegime, data),
            (h, g) => h.WorldPos().DistanceTo(g.WorldPos()), out var pCost);
        cost = pCost;
        return path;
    }

    public HashSet<Hex> GetMoveRadius(Hex start, Regime movingRegime, 
        float maxCost,
        HexGeneralData data)
    {
        var radius = PathFinder<Hex>.FloodFill(
            start, h => h.GetNeighbors(data),
            (h, g) => GetMoveCost(h, g, movingRegime, data),
            maxCost);
        return radius.Where(h => h.Full(data) == false).ToHashSet();
    }
}