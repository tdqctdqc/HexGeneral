using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public struct HexRef(Vector3I coords) : IRef<Hex>
{
    public Vector3I Coords { get; private set; } = coords;

    public Hex Get(Data d)
    {
        return d.Data().Map.Hexes[Coords];
    }

    public Vector2 GetWorldPos()
    {
        return Coords.GetWorldPos();
    }
}