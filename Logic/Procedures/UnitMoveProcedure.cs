using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game;
using HexGeneral.Game.Components;

namespace HexGeneral.Logic.Procedure;

public class UnitMoveProcedure(ERef<Unit> unit, List<HexRef> path, float moveCost, bool mobilized) : GodotUtilities.Server.Procedure
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public List<HexRef> Path { get; private set; } = path;
    public float MoveCost { get; private set; } = moveCost;
    public bool Mobilized { get; private set; } = mobilized;

    public override void Handle(ProcedureKey key)
    {
        var hexes = key.Data.Data().Map.Hexes;
        var regime = Unit.Get(key.Data).Regime;
        var unit = Unit.Get(key.Data);
        foreach (var r in Path)
        {
            var hex = hexes[r.Coords];
            hex.SetRegime(regime, key);
        }
        key.Data.Data().MapUnitHolder.SetUnitPosition(unit,
            Path[^1], key);
        unit.MarkMoved(MoveCost, key);
        if (Mobilized)
        {
            unit.Components.OfType<MobilizerComponent>().Single()
                .MarkActive(key);
        }
        key.Data.Data().Notices.UnitMoved?.Invoke(this);
    }
}