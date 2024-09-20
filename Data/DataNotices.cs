using System;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.RefAction;
using HexGeneral.Logic.Procedure;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game;

public class DataNotices
{
    public Action<Player> PlayerChangedRegime { get; set; }

    public RefAction TurnStarted { get; set; }
        = new RefAction();
    public RefAction FinishedTurnStartLogic { get; set; }
        = new RefAction();

    public RefAction<UnitMoveProcedure> UnitMoved { get; set; }
        = new RefAction<UnitMoveProcedure>();
    public Action<UnitAttackProcedure> UnitAttacked { get; set; }
    public Action<UnitRetreatProcedure> UnitRetreated { get; set; }
    public Action<Unit, Hex> UnitDestroyed { get; set; }
    
    public RefAction<Unit> UnitAltered { get; set; }
        = new RefAction<Unit>();
    public RefAction<Unit> UnitDeployed { get; private set; }
        = new RefAction<Unit>();
    
    public Action<Regime> ResourcesAltered { get; set; }
}