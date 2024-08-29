using System;
using System.Collections.Generic;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class RoadNetwork : Entity
{
    public Dictionary<long, ModelRef<RoadModel>> I { get; private set; }
    public RoadNetwork(int id) : base(id)
    {
    }

    public override void Made(Data d)
    {
        d.SetEntitySingleton<RoadNetwork>();
    }

    public override void CleanUp(Data d)
    {
        throw new Exception();
    }
}