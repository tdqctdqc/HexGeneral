using System;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Data.Components;
using HexGeneral.Game;

namespace HexGeneral.Logic.Procedures;

public class WorkOnRoadProcedure(Vector2I edge, ERef<Unit> unit, float progress, ModelIdRef<RoadModel> roadModel)
    : GodotUtilities.Server.Procedure
{
    public Vector2I Edge { get; private set; } = edge;
    public ERef<Unit> Unit { get; private set; } = unit;
    public float Progress { get; private set; } = progress;
    public ModelIdRef<RoadModel> RoadModel { get; private set; } = roadModel;

    public override void Handle(ProcedureKey key)
    {
        var data = key.Data.Data();
        
        Unit.Get(data).Components
            .Get<EngineerEntityComponent>(data)
            .SpendPoints(Progress, key);
        
        var roadNetwork = data.RoadNetwork;
        var roadModel = RoadModel.Get(data);
        if (roadNetwork.Roads.TryGetValue(Edge, 
                out var existing))
        {
            if (existing.Get(data).Level != roadModel.Level + 1)
            {
                throw new Exception();
            }
        }
        else if (roadModel.Level != 0)
        {
            throw new Exception();
        }
        
        var projects = data.EngineerProjects;
        var newProgress = Progress;
        if (projects.RoadConstructionProgresses.TryGetValue(Edge, out var v))
        {
            var oldProgress = v.Item2;
            newProgress += oldProgress;
        }
        projects.RoadConstructionProgresses[Edge] 
            = (roadModel.MakeIdRef(data), newProgress);

        if (projects.RoadConstructionProgresses[Edge].Item2 >= roadModel.EngineerPointCost)
        {
            roadNetwork.AddRoad(Edge, RoadModel.Get(data), key);
            projects.RoadConstructionProgresses.Remove(Edge);
        }
    }
}