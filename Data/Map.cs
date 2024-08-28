using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;
using MessagePack;

namespace HexGeneral.Game;

public class Map : Entity
{
    

    public Dictionary<Vector3I, Hex> Hexes { get; private set; }
    public static Map Create(Data d)
    {
        var map = new Map(d.IdDispenser.TakeId(),
            new Dictionary<Vector3I, Hex>());
        d.Entities.AddEntity(map, d);
        return map;
    }
    [SerializationConstructor] private Map(int id, Dictionary<Vector3I, Hex> hexes) : base(id)
    {
        Hexes = hexes;
    }
    
    public override void Made(Data d)
    {
        
    }

    public override void CleanUp(Data d)
    {
        
    }
}