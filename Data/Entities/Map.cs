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
    public Vector2I GridBounds { get; private set; }
    public static Map Create(Vector2I gridBounds, GodotUtilities.GameData.Data d)
    {
        var map = new Map(d.IdDispenser.TakeId(),
            new Dictionary<Vector3I, Hex>(),
            new Dictionary<int, Vector3I>(),
            gridBounds);
        d.Entities.AddEntity(map, d);
        return map;
    }

    public bool InGridBounds(Vector2I coord)
    {
        return coord.X >= 0
               && coord.X < GridBounds.X
               && coord.Y >= 0
               && coord.Y < GridBounds.Y;
    }
    [SerializationConstructor] private Map(int id, Dictionary<Vector3I, Hex> hexes, Dictionary<int, Vector3I> coordsById, Vector2I gridBounds) : base(id)
    {
        Hexes = hexes;
        CoordsById = coordsById;
        GridBounds = gridBounds;
    }
    
    public override void Made(GodotUtilities.GameData.Data d)
    {
        d.SetEntitySingleton<Map>();
    }

    public override void CleanUp(GodotUtilities.GameData.Data d)
    {
        throw new Exception();
    }
    
}