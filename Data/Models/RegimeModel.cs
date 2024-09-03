using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class RegimeModel : Model
{
    public Color Color { get; private set; }
    public RegimeModel(string name) : base(name)
    {
    }
}