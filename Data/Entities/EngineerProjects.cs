using System.Collections.Generic;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class EngineerProjects : Entity
{
    
    public Dictionary<Vector2I, (ModelIdRef<RoadModel>, float)> RoadConstructionProgresses { get; private set; }
    public Dictionary<HexRef, 
        Dictionary<ModelIdRef<ConstructableBuildingModel>, float>> BuildingConstructionProgresses { get; private set; }
    
    public EngineerProjects(int id,
        Dictionary<Vector2I, (ModelIdRef<RoadModel>, float)> roadConstructionProgresses, 
        Dictionary<HexRef, 
            Dictionary<ModelIdRef<ConstructableBuildingModel>, float>> buildingConstructionProgresses) : base(id)
    {
        RoadConstructionProgresses = roadConstructionProgresses;
        BuildingConstructionProgresses = buildingConstructionProgresses;
    }

    public override void Made(GodotUtilities.GameData.Data d)
    {
        d.SetEntitySingleton<EngineerProjects>();
    }

    public override void CleanUp(GodotUtilities.GameData.Data d)
    {
        throw new System.Exception();
    }

    public Control GetDescrForHex(Hex hex, HexGeneralData data)
    {
        var vbox = new VBoxContainer();

        if (BuildingConstructionProgresses.TryGetValue(hex.MakeRef(),
                out var buildingProgresses))
        {
            foreach (var (modelRef, progress) in buildingProgresses)
            {
                var building = modelRef.Get(data);
                var cost = building.EngineerPointCost;
                vbox.CreateLabelAsChild($"{building.Name}: {progress} / {cost}");
            }
        }
        foreach (var neighbor in hex.GetNeighbors(data))
        {
            if (RoadConstructionProgresses.TryGetValue(hex.GetIdEdgeKey(neighbor),
                    out var v))
            {
                var (road, progress) = v;
                vbox.CreateLabelAsChild($"Building {road.Get(data).Name} to {neighbor.GetGridCoords()}");
                vbox.CreateLabelAsChild($"Progress {progress} / {road.Get(data).EngineerPointCost}");
            }
        }
        return vbox;
    }

    public bool GetBuildingProgress(ConstructableBuildingModel model, 
        Hex hex, 
        HexGeneralData data,
        out float progress)
    {
        progress = 0f;
        if (BuildingConstructionProgresses.TryGetValue(hex.MakeRef(),
                out var progresses))
        {
            if (progresses.TryGetValue(model.MakeIdRef(data), out var bProgress))
            {
                progress = bProgress;
                return true;
            }
        }

        return false;
    }

    public bool GetRoadProgress(RoadModel model,
        Vector2I edge,
        HexGeneralData data,
        out float progress)
    {
        progress = 0f;
        if (RoadConstructionProgresses.TryGetValue(edge,
                out var v))
        {
            
            if (v.Item1.Get(data) == model)
            {
                progress = v.Item2;
                return true;
            }
        }

        return false;
    }
}