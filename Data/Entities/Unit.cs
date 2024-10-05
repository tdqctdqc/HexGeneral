using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Data.Components;
using HexGeneral.Game.Components;
using MessagePack;

namespace HexGeneral.Game;

public class Unit : Entity, IComponentedEntity
{
    public ERef<Regime> Regime { get; private set; }
    public ModelIdRef<UnitModel> UnitModel { get; private set; }
    public float CurrentHitPoints { get; private set; }
    public bool Reinforced { get; private set; }
    public EntityComponentHolder Components { get; private set; }
    public static Unit Instantiate(UnitModel model, 
        Regime regime, HexGeneralData data)
    {
        var id = data.IdDispenser.TakeId();
        var uRef = new ERef<Unit>(id);
        var moveComp = new NativeMoveComponent(uRef);
        var org = new OrganizationComponent(uRef, model.Organization);
        var attacksCount = new AttackCountComponent(0, 
            1);
        var movesTaken = new MoveCountComponent(0, 
            1, 1f);
        
        var u = new Unit(id,
            regime.MakeRef(), model.HitPoints, 
            model.MakeIdRef(data), false,
            EntityComponentHolder.Construct());
        
        u.Components.Initialize(u,
            new List<IEntityComponent> { 
                moveComp, 
                movesTaken, 
                attacksCount,
                org
            }, data, model);
        
        return u;
    }
    [SerializationConstructor] private Unit(int id, ERef<Regime> regime, float currentHitPoints,
        ModelIdRef<UnitModel> unitModel,
        bool reinforced,
        EntityComponentHolder components) : base(id)
    {
        Regime = regime;
        UnitModel = unitModel;
        CurrentHitPoints = currentHitPoints;
        Reinforced = reinforced;
        Components = components;
    }

    

    public override void Made(GodotUtilities.GameData.Data d)
    {
        
    }

    public override void CleanUp(GodotUtilities.GameData.Data d)
    {
        d.Data().MapUnitHolder.Remove(this, d.Data());
    }

    public void Reinforce(float amount, ProcedureKey key)
    {
        CurrentHitPoints += amount;
        Reinforced = true;
    }

    public void IncrementHitpoints(float amount, ProcedureKey key)
    {
        CurrentHitPoints += amount;
        CurrentHitPoints = Mathf.Clamp(CurrentHitPoints, 0f, UnitModel.Get(key.Data).HitPoints);
    }
    
    public void RefreshForTurn(ProcedureKey key)
    {
        Reinforced = false;
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

    public bool CanReinforce(HexGeneralData data)
    {
        return Components.Get<MoveCountComponent>(data).HasMoved() == false 
               && Reinforced == false;
    }
}