using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.GameData;
using GodotUtilities.Serialization;

namespace HexGeneral.Game;

public class HexGeneralData : Data
{
    public ModelPredefs ModelPredefs { get; private set; } 
    public Map Map => (Map)Singletons[typeof(Map)];
    public RoadNetwork RoadNetwork => (RoadNetwork)Singletons[typeof(RoadNetwork)];
    public LandSeaMasses LandSeaMasses => (LandSeaMasses)Singletons[typeof(LandSeaMasses)];
    public MapUnitHolder MapUnitHolder => (MapUnitHolder)Singletons[typeof(MapUnitHolder)];
    
    
    public HexGeneralData() 
        : base(new(0),
        new(new Dictionary<int, Entity>()),
        new(),
        new(),
        new(),
        new())
    {
        ModelPredefs = new ModelPredefs(this);
    }
}