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
    void DrawPath(Unit unit, Hex dest, MapOverlayDrawer mesh, HexGeneralData data);
    Command GetMoveCommand(Unit unit, Hex dest, HexGeneralClient client);
    float GetMovePoints(HexGeneralData data);
    MoveType GetActiveMoveType(HexGeneralData data);
    bool AttackBlocked(HexGeneralData data);
}   