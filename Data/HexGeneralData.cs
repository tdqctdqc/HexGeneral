using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.GameData;
using GodotUtilities.Serialization;

namespace HexGeneral.Game;

public class HexGeneralData : GodotUtilities.GameData.Data
{
    public ModelPredefs ModelPredefs { get; private set; } 
    public Map Map => (Map)Singletons[typeof(Map)];
    public RoadNetwork RoadNetwork => (RoadNetwork)Singletons[typeof(RoadNetwork)];
    public LandSeaMasses LandSeaMasses => (LandSeaMasses)Singletons[typeof(LandSeaMasses)];
    public MapUnitHolder MapUnitHolder => (MapUnitHolder)Singletons[typeof(MapUnitHolder)];
    public LocationHolder LocationHolder => (LocationHolder)Singletons[typeof(LocationHolder)];
    public PlayerHolder PlayerHolder => (PlayerHolder)Singletons[typeof(PlayerHolder)];
    public TurnManager TurnManager => (TurnManager)Singletons[typeof(TurnManager)];
    public GameSettings Settings { get; private set; }
    public GenerationSettings GenerationSettings { get; private set; }
    public DataNotices Notices { get; private set; }
    public HexGeneralData(GameSettings settings, GenerationSettings generationSettings) 
        : base(new(0),
        new(new Dictionary<int, Entity>()),
        new(),
        new(),
        new(),
        new())
    {
        Notices = new DataNotices();
        Settings = settings;
        GenerationSettings = generationSettings;
        ModelPredefs = new ModelPredefs(this);
    }
}