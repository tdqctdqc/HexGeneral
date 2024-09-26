using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game;
using HexGeneral.Game.Components;

namespace HexGeneral.Logic.Procedure;

public class UnitMoveProcedure(ERef<Unit> unit, List<HexRef> path,
    float moveRatioSpent) : GodotUtilities.Server.Procedure
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public List<HexRef> Path { get; private set; } = path;
    public float MoveRatioSpent { get; private set; } = moveRatioSpent;

    public override void Handle(ProcedureKey key)
    {
        var hexes = key.Data.Data().Map.Hexes;
        var unit = Unit.Get(key.Data);
        var moveType = unit.Components.Get<IMoveComponent>(key.Data)
            .GetActiveMoveType(key.Data.Data());
        foreach (var r in Path)
        {
            var hex = hexes[r.Coords];
            moveType.MoveThru(hex, unit, key);
        }
        key.Data.Data().MapUnitHolder.SetUnitPosition(unit,
            Path[^1], key);
        unit.Components.Get<MoveCountComponent>(key.Data)
            .SpendMove(MoveRatioSpent, key);
        key.Data.Data().Notices.UnitMoved?.Invoke(this);
    }
}