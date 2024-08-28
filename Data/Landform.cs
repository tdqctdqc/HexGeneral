using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class Landform : Model
{
    public bool IsLand { get; private set; }
    public Color Color { get; private set; }

    public Landform(string name) : base(name)
    {
        
    }
}