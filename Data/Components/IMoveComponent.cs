using System;
using System.Collections.Generic;
using GodotUtilities.Logic;
using GodotUtilities.Server;
using HexGeneral.Game.Client;
using HexGeneral.Logic.Procedure;

namespace HexGeneral.Game.Components;

public interface IMoveComponent : IUnitCombatComponent
{
    HashSet<Hex> GetMoveRadius(Unit unit, HexGeneralData data);
    void DrawRadius(Unit unit, MapOverlayDrawer mesh, HexGeneralData data);
    void TryMoveCommand(Unit unit, Hex dest, 
        Action<Command> submit,
        HexGeneralClient client);
    
    float GetMovePoints(HexGeneralData data);
    MoveType GetActiveMoveType(HexGeneralData data);
}   