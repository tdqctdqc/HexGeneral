using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game.Components;
using MessagePack;

namespace HexGeneral.Game;

public class Unit : Entity, IComponentedEntity
{
    public ERef<Regime> Regime { get; private set; }
    public ModelIdRef<UnitModel> UnitModel { get; private set; }
    public float CurrentHitPoints { get; private set; }
    public float CurrentOrganization { get; private set; }
    public int CurrentAmmo { get; private set; }
    public bool Reinforced { get; private set; }
    public bool RestockedAmmo { get; private set; }
    public EntityComponentHolder Components { get; private set; }
    public static Unit Instantiate(UnitModel model, 
        Regime regime, HexGeneralData data)
    {
        var id = data.IdDispenser.TakeId();
        var moveComp = model.MoveType.MakeNativeMoveComponent(new ERef<Unit>(id));
        var attacksCount = new AttackCountComponent(0, model.AttackType.NumAttacks);
        var movesTaken = new MoveCountComponent(0, 1, 1f);
        var compHolder = new EntityComponentHolder(
            new List<IEntityComponent> { moveComp, movesTaken, attacksCount });
        var u = new Unit(id,
            regime.MakeRef(), model.HitPoints, model.Organization, model.AmmoCap,
            model.MakeIdRef(data), false, false,
            compHolder);
        return u;
    }
    [SerializationConstructor] private Unit(int id, ERef<Regime> regime, float currentHitPoints,
        float currentOrganization, int currentAmmo, 
        ModelIdRef<UnitModel> unitModel,
        bool reinforced,
        bool restockedAmmo,
        EntityComponentHolder components) : base(id)
    {
        Regime = regime;
        UnitModel = unitModel;
        CurrentHitPoints = currentHitPoints;
        CurrentOrganization = currentOrganization;
        CurrentAmmo = currentAmmo;
        Reinforced = reinforced;
        RestockedAmmo = restockedAmmo;
        Components = components;
    }

    

    public override void Made(Data d)
    {
        
    }

    public override void CleanUp(Data d)
    {
        d.Data().MapUnitHolder.Remove(this);
    }

    public void Reinforce(float amount, ProcedureKey key)
    {
        CurrentHitPoints += amount;
        Reinforced = true;
    }

    public void RestockAmmo(int amount, ProcedureKey key)
    {
        CurrentAmmo += amount;
        RestockedAmmo = true;
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
        Reinforced = false;
        RestockedAmmo = false;
        RegenerateOrganization(key);
        Components.TurnTick(key);
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

    public bool Deployed(HexGeneralData data)
    {
        return data.MapUnitHolder.UnitPositions.ContainsKey(this.MakeRef());
    }

    public bool CanReinforce()
    {
        return Components.Get<MoveCountComponent>().HasMoved() == false 
               && Reinforced == false;
    }
    public bool CanRestockAmmo()
    {
        return Components.Get<MoveCountComponent>().HasMoved() == false 
               && RestockedAmmo == false;
    }
}