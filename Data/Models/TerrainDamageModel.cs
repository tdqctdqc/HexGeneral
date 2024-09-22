using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class TerrainDamageModel : Model
{
    public Dictionary<Landform, float> LandformDamageMults { get; private set; }
    public Dictionary<Vegetation, float> VegetationDamageMults { get; private set; }

    public float GetMult(Hex hex, Data data)
    {
        var lfMod = LandformDamageMults[hex.Landform.Get(data)];
        var vMod = VegetationDamageMults[hex.Vegetation.Get(data)];
        return lfMod + vMod;
    }
}