using System;
using System.Collections.Generic;
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
        var data = new HexGeneralData(gameSettings, generationSettings);
        
        GodotUtilities.GameData.Data.SetupForHost(data, 
            new HexGeneralModelImporter(data.ModelPredefs));
        var playerHolder = new PlayerHolder(data.IdDispenser.TakeId(), new Dictionary<Guid, ERef<Player>>());
        data.Entities.AddEntity(playerHolder, data);
        
        MapGenerator.Generate(data, setupData);
        RegimeGenerator.Generate(data, setupData);
        
        return data;
    }
}