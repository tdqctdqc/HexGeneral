using System.Linq;
using Godot;
using GodotUtilities.DataStructures.PathFinder;

namespace HexGeneral.Game.Logic;

public class SupplyLogic
{
    public static float HalfSupplyRange { get; } = 1f;

    public static float GetSupplyAvailabilityFromMoveCost(float cost)
    {
        var supplyCost = Mathf.Pow(.5f, cost / HalfSupplyRange);
        return 1f - Mathf.Clamp(1f - supplyCost, 0f, 1f);
    }
    public static float GetSupplyAvailability(Hex hex, HexGeneralData data)
    {
        var supplyMove = data.ModelPredefs.MoveTypes.SupplyMoveType;
        var regime = hex.Regime.Get(data);
        var closestSupply = PathFinder<Hex>.FindClosest(
            hex, h => h.GetNeighbors(data).Where(n => n.Regime == hex.Regime),
            (h, g) => supplyMove.GetMoveCost(h, g, regime, data),
            h => h.TryGetLocation(data, out var loc) 
                 && loc.Buildings.Any(b => b.Get(data) is ISupplyCenter),
            out var cost);
        if (closestSupply == null) return 0f;
        return GetSupplyAvailabilityFromMoveCost(cost);
    }
}