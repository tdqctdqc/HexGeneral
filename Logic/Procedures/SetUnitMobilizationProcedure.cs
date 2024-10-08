using System.Linq;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game;
using HexGeneral.Game.Components;

namespace HexGeneral.Logic.Procedures;

public class SetUnitMobilizationProcedure(ERef<Unit> unit, bool active) : GodotUtilities.Server.Procedure
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public bool Active { get; private set; } = active;

    public override void Handle(ProcedureKey key)
    {
        var unit = Unit.Get(key.Data);
        var mob = unit.Components
            .Get<MobilizerComponent>(key.Data);
        if (Active)
        {
            mob.Activate(key);
        }
        else
        {
            mob.Deactivate(key);
        }

        key.Data.Data().Notices.UnitAltered?.Invoke(unit);
    }
}