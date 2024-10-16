using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.GameData;
using GodotUtilities.Logger;
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
    public EngineerProjects EngineerProjects => (EngineerProjects)Singletons[typeof(EngineerProjects)];
    public GameSettings Settings { get; private set; }
    public DataNotices Notices { get; private set; }
    
    public HexGeneralData(GameSettings settings) 
        : base(new(0),
        new(new Dictionary<int, Entity>()),
        new(),
        new(),
        new(),
        new())
    {
        Notices = new DataNotices();
        Settings = settings;
        ModelPredefs = new ModelPredefs(this);
        Logger = new Logger(this, d => d.Data().TurnManager.RoundNumber);
    }
}