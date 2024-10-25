using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.GameData;
using GodotUtilities.Serialization;

namespace HexGeneral.Game.Generators;

public static class NewGameGenerator
{
    public static HexGeneralData Generate(GenerationSettings generationSettings)
    {
        
        var setupData = new GenerationData(generationSettings);
        var gameSettings = new GameSettings();
        var data = new HexGeneralData(gameSettings);
        
        GodotUtilities.GameData.Data.SetupForHost(data, 
            new HexGeneralModelImporter(data.ModelPredefs));
        var playerHolder = new PlayerHolder(data.IdDispenser.TakeId(), new Dictionary<Guid, ERef<Player>>());
        data.Entities.AddEntity(playerHolder, data);
        var engineerProjects = new EngineerProjects(data.IdDispenser.TakeId(),
            new Dictionary<Vector2I, (ModelIdRef<RoadModel>, float)>(),
            new Dictionary<HexRef, Dictionary<ModelIdRef<ConstructableBuildingModel>, float>>());
        data.Entities.AddEntity(engineerProjects, data);
        var combatNet = new CombatNet(data.IdDispenser.TakeId(),
            new TwoWay<ERef<Unit>, HexRef>(new Dictionary<HexRef, HashSet<ERef<Unit>>>(),
                new Dictionary<ERef<Unit>, HashSet<HexRef>>()));
        data.Entities.AddEntity(combatNet, data);
        
        MapGenerator.Generate(data, setupData);
        RegimeGenerator.Generate(data, setupData);
        
        return data;
    }
}