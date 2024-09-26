using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game.Components;

namespace HexGeneral.Game;

public class MapUnitHolder : Entity
{

    public static MapUnitHolder Construct(HexGeneralData data)
    {
        var h = new MapUnitHolder(data.IdDispenser.TakeId(),
            new Dictionary<ERef<Unit>, HexRef>(),
            new Dictionary<ModelIdRef<Domain>, Dictionary<HexRef, List<ERef<Unit>>>>());
        
        foreach (var domain in data.Models.GetModels<Domain>())
        {
            h.HexUnitsByDomain.Add(domain.MakeIdRef(data),
                new Dictionary<HexRef, List<ERef<Unit>>>());
        }

        return h;
    }
    private MapUnitHolder(int id, 
        Dictionary<ERef<Unit>, HexRef> unitPositions, 
        Dictionary<ModelIdRef<Domain>,
            Dictionary<HexRef, List<ERef<Unit>>>> hexUnitsByDomain) : base(id)
    {
        UnitPositions = unitPositions;
        HexUnitsByDomain = hexUnitsByDomain;
    }

    public static int MaxUnitsPerHex { get; private set; } = 3;
    public Dictionary<ERef<Unit>, HexRef> UnitPositions { get; private set; }

    public Dictionary<ModelIdRef<Domain>,
        Dictionary<HexRef, List<ERef<Unit>>>> HexUnitsByDomain { get; private set; }
    
    
    public void DeployUnit(Unit u, Hex hex, 
        HexGeneralData data)
    {
        if (UnitPositions.ContainsKey(u.MakeRef()))
        {
            throw new Exception();
        }

        var domain = u.Components.Get<IMoveComponent>(data)
            .GetActiveMoveType(data).Domain;

        HexUnitsByDomain[domain.MakeIdRef(data)]
            .AddOrUpdate(hex.MakeRef(), u.MakeRef());
        UnitPositions.Add(u.MakeRef(), hex.MakeRef());
    }

    public void SetUnitPosition(Unit u, HexRef pos, ProcedureKey key)
    {
        var data = key.Data.Data();
        var domain = u.Components.Get<IMoveComponent>(data)
            .GetActiveMoveType(data).Domain;
        var units = HexUnitsByDomain[domain.MakeIdRef(data)];
        if (UnitPositions
            .TryGetValue(u.MakeRef(), out var prev))
        {
            units[prev].Remove(u.MakeRef());
        }

        units.AddOrUpdate(pos, u.MakeRef());
        UnitPositions[u.MakeRef()] = pos;
    }
    public override void Made(GodotUtilities.GameData.Data d)
    {
        d.SetEntitySingleton<MapUnitHolder>();
    }

    public override void CleanUp(GodotUtilities.GameData.Data d)
    {
        throw new System.Exception();
    }

    
    public void Remove(Unit u, HexGeneralData data)
    {
        var r = u.MakeRef();
        var domain = u.Components.Get<IMoveComponent>(data)
            .GetActiveMoveType(data).Domain;
        var units = HexUnitsByDomain[domain.MakeIdRef(data)];
        
        if (UnitPositions.Remove(r, out var coords))
        {
            units[coords].Remove(r);
        }
    }

    public void SwitchUnitDomain(Unit unit, Domain newDomain, HexGeneralData data)
    {
        var uRef = unit.MakeRef();
        var oldDomain = unit.Components.Get<IMoveComponent>(data)
            .GetActiveMoveType(data).Domain.MakeIdRef(data);
        var hex = UnitPositions[uRef];
        if (HexUnitsByDomain[oldDomain][hex].Remove(uRef)
            == false)
        {
            throw new Exception();
        }

        if (hex.Get(data).Full(newDomain, data))
        {
            throw new Exception();
        }

        HexUnitsByDomain[newDomain.MakeIdRef(data)].AddOrUpdate(hex, uRef);
        
    }
}