using System.Collections.Generic;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Data.Components;
using HexGeneral.Game;
using HexGeneral.Game.Components;

namespace HexGeneral.Logic.Procedures;

public class DisembarkProcedure(ERef<Unit> unit, HexRef intoHex) : GodotUtilities.Server.Procedure
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public HexRef IntoHex { get; private set; } = intoHex;

    public override void Handle(ProcedureKey key)
    {
        var unit = Unit.Get(key.Data);
        var from = unit.GetHex(key.Data.Data());
        key.Data.Data().MapUnitHolder.SetUnitPosition(unit,
            IntoHex, key);
        new RemoveEntityComponentProcedure<Unit, EmbarkedComponent>(Unit).Handle(key);
        unit.Components.Get<MoveCountComponent>(key.Data)
            .SpendMove(1f, key);
        key.Data.Data().Notices.UnitMoved?.Invoke((unit, new List<HexRef>{from.MakeRef(), IntoHex}));
    }
}