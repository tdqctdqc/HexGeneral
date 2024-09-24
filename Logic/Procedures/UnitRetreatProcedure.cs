using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game;
using HexGeneral.Game.Components;

namespace HexGeneral.Logic.Procedures;

public class UnitRetreatProcedure(HexRef from, HexRef to, 
    ERef<Unit> unit,
    int retreatDistance) : GodotUtilities.Server.Procedure
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public HexRef To { get; private set; } = to;
    public HexRef From { get; private set; } = from;
    public int RetreatDistance { get; private set; } = retreatDistance;

    public override void Handle(ProcedureKey key)
    {
        var data = key.Data.Data();
        var unit = Unit.Get(key.Data);
        key.Data.Data().MapUnitHolder.SetUnitPosition(unit,
            To, key);
        var model = unit.UnitModel.Get(data);
        unit.Components.Get<OrganizationComponent>()
            .IncrementOrganization(-model.Organization * RetreatDistance / 10f, key);
        data.Notices.UnitRetreated?.Invoke(this);
        data.Notices.UnitAltered?.Invoke(unit);
    }
}