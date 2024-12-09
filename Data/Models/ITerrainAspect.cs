using GodotUtilities.DataStructures;

namespace HexGeneral.Game;

public interface ITerrainAspect : INamed
{
    float DecalWidth { get; }
    float DecalDist { get; }
    
}