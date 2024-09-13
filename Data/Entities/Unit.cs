using System.Linq;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;

namespace HexGeneral.Game;

public class Unit(int id, ERef<Regime> regime, float currentHitPoints,
    float currentOrganization, int currentAmmo, ModelIdRef<UnitModel> unitModel, float movePoints, bool moved, bool attacked) : Entity(id)
{
    public ERef<Regime> Regime { get; private set; } = regime;
    public ModelIdRef<UnitModel> UnitModel { get; private set; } = unitModel;
    public float MovePoints { get; private set; } = movePoints;
    public float CurrentHitPoints { get; private set; } = currentHitPoints;
    public float CurrentOrganization { get; private set; } = currentOrganization;
    public int CurrentAmmo { get; private set; } = currentAmmo;
    public bool Moved { get; private set; } = moved;
    public bool Attacked { get; private set; } = attacked;

    public override void Made(Data d)
    {
        
    }

    public override void CleanUp(Data d)
    {
        d.Data().MapUnitHolder.Remove(this);
    }

    public void MarkMoved(float movePointsSpent, ProcedureKey key)
    {
        MovePoints -= movePointsSpent;
        Moved = true;
    }

    public void MarkHasAttacked(ProcedureKey key)
    {
        Attacked = true;
    }

    public void IncrementHitpoints(float amount, ProcedureKey key)
    {
        CurrentHitPoints += amount;
        CurrentHitPoints = Mathf.Clamp(CurrentHitPoints, 0f, UnitModel.Get(key.Data).HitPoints);
    }

    public void RegenerateOrganization(ProcedureKey key)
    {
        var data = key.Data.Data();
        var holder = data.MapUnitHolder.UnitPositions;
        float regenRatio;
        if (holder.TryGetValue(this.MakeRef(), out var hRef))
        {
            var hex = hRef.Get(data);
            var hostile = hex.GetNeighbors(data)
                .Where(n => n.Regime.Fulfilled() && n.Regime != Regime);
            
            if (hostile.Any())
            {
                if (hostile.Any(n => n.GetUnitRefs(data).Any()))
                {
                    regenRatio = .15f;
                }
                else
                {
                    regenRatio = .25f;
                }
            }
            else
            {
                regenRatio = .4f;
            }
        }
        else
        {
            regenRatio = .4f;
        }
        IncrementOrganization(UnitModel.Get(data).Organization * regenRatio, key);
    }
    public void IncrementOrganization(float amount, ProcedureKey key)
    {
        CurrentOrganization += amount;
        CurrentOrganization = Mathf.Clamp(CurrentOrganization, 0f, UnitModel.Get(key.Data).Organization);
    }
    public void IncrementAmmo(int amount, ProcedureKey key)
    {
        CurrentAmmo += amount;
        CurrentAmmo = Mathf.Clamp(CurrentAmmo, 0, UnitModel.Get(key.Data).AmmoCap);
    }
    public void RefreshForTurn(ProcedureKey key)
    {
        Moved = false;
        Attacked = false;
        MovePoints = UnitModel.Get(key.Data).MovePoints;
        RegenerateOrganization(key);
    }

    public Hex GetHex(HexGeneralData data)
    {
        if (data.MapUnitHolder.UnitPositions.TryGetValue(this.MakeRef(), 
                out var r))
        {
            return data.Map.Hexes[r.Coords];
        }
        return null;
    }
}