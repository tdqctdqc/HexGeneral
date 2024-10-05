using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Data.Components;
using HexGeneral.Game;

namespace HexGeneral.Logic.Procedures;

public class WorkOnBuildingProcedure(HexRef hex, ERef<Unit> unit, float progress, ModelIdRef<ConstructableBuildingModel> buildingModel) : GodotUtilities.Server.Procedure
{
    public HexRef Hex { get; private set; } = hex;
    public ERef<Unit> Unit { get; private set; } = unit;
    public float Progress { get; private set; } = progress;
    public ModelIdRef<ConstructableBuildingModel> BuildingModel { get; private set; } = buildingModel;

    public override void Handle(ProcedureKey key)
    {
        var data = key.Data.Data();
        
        Unit.Get(data).Components
            .Get<EngineerEntityComponent>(data)
            .SpendPoints(Progress, key);
        
        Hex.Get(data).TryGetLocation(data, out var loc);
        
        var projects = data.EngineerProjects;
        var newProgress = Progress;
        if (projects.BuildingConstructionProgresses
            .TryGetValue(Hex, out var vs))
        {
            if (vs.TryGetValue(BuildingModel, out var oldProgress))
            {
                newProgress += oldProgress;
            }
        }
        else
        {
            projects.BuildingConstructionProgresses.Add(Hex,
                new Dictionary<ModelIdRef<ConstructableBuildingModel>, float>());
        }
        projects.BuildingConstructionProgresses[Hex][BuildingModel]
            = newProgress;

        if (newProgress >= BuildingModel.Get(data).EngineerPointCost)
        {
            loc.Buildings.Add(BuildingModel.Get(data)
                .MakeIdRef<BuildingModel>(data));
            projects.BuildingConstructionProgresses[Hex].Remove(BuildingModel);
            if (projects.BuildingConstructionProgresses[Hex].Count == 0)
            {
                projects.BuildingConstructionProgresses.Remove(Hex);
            }
        }
        else
        {
            projects.BuildingConstructionProgresses[Hex][BuildingModel] = newProgress;
        }
        data.Notices.HexAltered?.Invoke(Hex.Get(data));
    }
}