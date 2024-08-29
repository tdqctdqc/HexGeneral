using System.Collections.Generic;
using GodotUtilities.DataStructures;
using GodotUtilities.GameData;
using GodotUtilities.Serialization;

namespace HexGeneral.Game.Generators;

public static class NewGameGenerator
{
    public static HexGeneralData Generate(NewGameSettings settings)
    {
        var data = new HexGeneralData();
            
        GodotUtilities.GameData.Data.SetupForHost(data, 
            new HexGeneralModelImporter(data.ModelPredefs));
        
        var map = MapGenerator.Generate(data, settings);
        

        return data;
    }
}