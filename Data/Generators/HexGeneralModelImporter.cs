using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Reflection;
using GodotUtilities.Serialization.Depot;
using HexGeneral.Game.Components;

namespace HexGeneral.Game.Generators;

public class HexGeneralModelImporter(ModelPredefs predefs) 
    : ModelImporter("hexGeneral.dpo")
{
    private ModelPredefs _predefs = predefs;

    public override void Import(Models models)
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        var modelTypes = typeof(Model)
            .GetDerivedTypes(types)
            .Distinct();
        var componentTypes = typeof(IModelComponent)
            .GetConcreteTypesOfType(types)
            .Distinct();
        var importer = new DepotImporter(_path, modelTypes, componentTypes,
            models);
    }

}