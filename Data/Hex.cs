using Godot;

namespace HexGeneral.Game;

public class Hex
{
    public Vector3I Coords { get; }

    public Hex(Vector3I coords)
    {
        Coords = coords;
    }
}