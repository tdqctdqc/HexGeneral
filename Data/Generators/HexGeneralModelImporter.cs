using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Serialization.Depot;

namespace HexGeneral.Game.Generators;

public class HexGeneralModelImporter(ModelPredefs predefs) : ModelImporter("hexGeneral.dpo")
{
    private ModelPredefs _predefs = predefs;

    protected override void SetupModelsSpecific(Models models, DepotImporter importer)
    {
        models.ImportWithPredefsAllowDefault<Landform>(
            _predefs.Landforms.GetPredefsByName(),
            n => new Landform(n),
            importer);
        
        models.ImportWithPredefsAllowDefault<Vegetation>(
            _predefs.Vegetations.GetPredefsByName(),
            n => new Vegetation(n),
            importer);
        
        importer.FillAllProperties();
    }

}