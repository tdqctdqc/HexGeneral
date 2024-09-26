using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class RoadNetwork(
    int id,
    Dictionary<Vector2I, ModelIdRef<RoadModel>> roads)
    : Entity(id)
{
    public Dictionary<Vector2I, ModelIdRef<RoadModel>> Roads { get; private set; } = roads;

    public override void Made(GodotUtilities.GameData.Data d)
    {
        d.SetEntitySingleton<RoadNetwork>();
    }

    public override void CleanUp(GodotUtilities.GameData.Data d)
    {
        throw new Exception();
    }
}