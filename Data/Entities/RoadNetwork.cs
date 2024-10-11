using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;

namespace HexGeneral.Game;

public class RoadNetwork(
    int id,
    Dictionary<Vector2I, ModelIdRef<RoadModel>> roads)
    : Entity(id), ISingletonEntity
{
    public Dictionary<Vector2I, ModelIdRef<RoadModel>> Roads { get; private set; } = roads;

    public override void Made(GodotUtilities.GameData.Data d)
    {
    }

    public override void CleanUp(GodotUtilities.GameData.Data d)
    {
        throw new Exception();
    }

    public void AddRoad(Vector2I edge, RoadModel r, ProcedureKey key)
    {
        Roads[edge] = r.MakeIdRef(key.Data);
        key.Data.Data().Notices.NewRoad.Invoke(edge);
    }
}