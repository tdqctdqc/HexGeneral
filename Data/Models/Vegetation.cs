using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class Vegetation : Model, ITerrainAspect
{
    public Color Color { get; private set; }
    public float MinMoisture { get; private set; }
    public float DecalWidth { get; private set; }
    public float DecalDist { get; private set; }
    public HashSet<Landform> AllowedLandforms { get; private set; }
    public Vegetation()
    {
    }
}