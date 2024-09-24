using GodotUtilities.GameData;
using GodotUtilities.Logic;
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
        var ammo = unit.Components.Get<AmmunitionComponent>();
        if (ammo.CanRestock(key.Data.Data()) == false) return;
        var regime = unit.Regime.Get(key.Data);
        var model = unit.UnitModel.Get(key.Data);
        var industrialCost = model.AmmoCost * Amount;
        ammo.IncrementAmmo(Amount, key);
        regime.IncrementIndustrialPoints(-industrialCost, key);
        key.Data.Data().Notices.UnitAltered?.Invoke(unit);
    }
}