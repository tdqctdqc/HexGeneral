using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game;

namespace HexGeneral.Logic.Procedures;

public class UnitDestroyedProcedure(ERef<Unit> unit) : GodotUtilities.Server.Procedure
{
    public ERef<Unit> Unit { get; private set; } = unit;

    public override void Handle(ProcedureKey key)
    {
        var unit = Unit.Get(key.Data);
        var hex = unit.GetHex(key.Data.Data());
        key.Data.Entities.RemoveEntity(unit.Id, key.Data);
        key.Data.Data().Notices.UnitDestroyed?.Invoke(unit, hex);
    }
}