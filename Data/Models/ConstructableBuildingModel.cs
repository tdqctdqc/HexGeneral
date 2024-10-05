using System.Collections.Generic;

namespace HexGeneral.Game;

public class ConstructableBuildingModel : BuildingModel 
{
    public float EngineerPointCost { get; private set; }
    public HashSet<Landform> AllowedLandforms { get; private set; }
}