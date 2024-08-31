using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Serialization.Depot;

namespace HexGeneral.Game.Generators;

public class HexGeneralModelImporter(ModelPredefs predefs) : ModelImporter("hexGeneral.dpo")
{
    private ModelPredefs _predefs = predefs;

    protected override void SetupModelsSpecific(Models models, DepotImporter importer)
    {
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
        importer.FillAllProperties();
    }

}