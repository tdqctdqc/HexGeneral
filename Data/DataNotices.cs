using System;
using GodotUtilities.DataStructures;
using HexGeneral.Logic.Procedure;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game;

public class DataNotices
{
    public Action<Player> PlayerChangedRegime { get; set; }
    public Action TurnStarted { get; set; }
    public Action FinishedTurnStartLogic { get; set; }
    public Action<UnitMoveProcedure> UnitMoved { get; set; }
    public Action<UnitAttackedProcedure> UnitAttacked { get; set; }
    public Action<UnitRetreatProcedure> UnitRetreated { get; set; }
    public Action<Unit, Hex> UnitDestroyed { get; set; }
    
}