using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;
using MessagePack;

namespace HexGeneral.Game;

public class Map : Entity
{
    public Dictionary<Vector3I, Hex> Hexes { get; private set; }
    public Dictionary<int, Vector3I> CoordsById { get; private set; }
    public static Map Create(Data d)
    {
        var map = new Map(d.IdDispenser.TakeId(),
            new Dictionary<Vector3I, Hex>(),
            new Dictionary<int, Vector3I>());
        d.Entities.AddEntity(map, d);
        return map;
    }
    [SerializationConstructor] private Map(int id, Dictionary<Vector3I, Hex> hexes, Dictionary<int, Vector3I> coordsById) : base(id)
    {
        Hexes = hexes;
        CoordsById = coordsById;
    }
    
    public override void Made(Data d)
    {
        d.SetEntitySingleton<Map>();
    }

    public override void CleanUp(Data d)
    {
        throw new Exception();
    }
    
}