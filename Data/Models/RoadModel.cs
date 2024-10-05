using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class RoadModel : Model
{
    public float EngineerPointCost { get; private set; }
    public Color Color { get; private set; }
    public int Level { get; private set; }
}