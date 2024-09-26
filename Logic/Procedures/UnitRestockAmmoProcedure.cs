using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Data.Components;
using HexGeneral.Game;
using HexGeneral.Game.Components;

namespace HexGeneral.Logic.Procedures;

public class UnitRestockAmmoProcedure(ERef<Unit> unit, int amount) : GodotUtilities.Server.Procedure
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public int Amount { get; private set; } = amount;

    public override void Handle(ProcedureKey key)
    {
        var unit = Unit.Get(key.Data);
        var currAmmo = unit.Components.Get<CurrentAmmunitionComponent>(key.Data);
        if (currAmmo.CanRestock(key.Data.Data()) == false) return;
        var regime = unit.Regime.Get(key.Data);
        var model = unit.UnitModel.Get(key.Data);
        var parent = model.Components.Get<AmmunitionComponent>();
        var industrialCost = parent.AmmoCost * Amount;
        currAmmo.IncrementAmmo(Amount, key);
        regime.IncrementIndustrialPoints(-industrialCost, key);
        key.Data.Data().Notices.UnitAltered?.Invoke(unit);
    }
}