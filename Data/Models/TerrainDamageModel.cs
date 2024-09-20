using System.Collections.Generic;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class TerrainDamageModel : Model
{
    public Dictionary<Landform, float> LandformDamageMults { get; private set; }
    public Dictionary<Vegetation, float> VegetationDamageMults { get; private set; }

    public float GetMult(Hex hex, Data data)
    {
        return LandformDamageMults[hex.Landform.Get(data)]
               * VegetationDamageMults[hex.Vegetation.Get(data)];
    }
}