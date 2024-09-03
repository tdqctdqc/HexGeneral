using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class Vegetation : Model
{
    public Color Color { get; private set; }
    public float MinMoisture { get; private set; }
    public HashSet<Landform> AllowedLandforms { get; private set; }
    public Vegetation()
    {
    }
}