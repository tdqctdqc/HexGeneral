using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Reflection;
using GodotUtilities.Serialization.Depot;

namespace HexGeneral.Game.Generators;

public class HexGeneralModelImporter(ModelPredefs predefs) 
    : ModelImporter("hexGeneral.dpo")
{
    private ModelPredefs _predefs = predefs;

    public override void Import(Models models)
    {
        var importer = new DepotImporter(_path);
        // var types = Assembly.GetExecutingAssembly().GetTypes();
        // var modelTypes = typeof(Model).GetDerivedTypes(types);
        // foreach (var modelType in modelTypes)
        // {
        //     
        // }
        models.ImportNoPredefs(
            n => new Landform(n),
            importer);
        models.ImportNoPredefs(
            n => new Vegetation(n),
            importer);
        models.ImportNoPredefs(
            n => new RoadModel(n),
            importer);
        models.ImportNoPredefs(
            n => new RegimeModel(n),
            importer);
        models.ImportNoPredefs(n => new UnitModel(n),
            importer);
        importer.FillAllProperties();
    }

}