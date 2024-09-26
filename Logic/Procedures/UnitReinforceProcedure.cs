using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game;

namespace HexGeneral.Logic.Procedures;

public class UnitReinforceProcedure(ERef<Unit> unit, float amount) : GodotUtilities.Server.Procedure
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public float Amount { get; private set; } = amount;

    public override void Handle(ProcedureKey key)
    {
        var unit = Unit.Get(key.Data);
        if (unit.CanReinforce(key.Data.Data()) == false) return;
        var regime = unit.Regime.Get(key.Data);
        var model = unit.UnitModel.Get(key.Data);
        var ratio = Amount / model.HitPoints;
        var recruitCost = model.RecruitCost * ratio;
        var industrialCost = model.IndustrialCost * ratio;
        unit.Reinforce(Amount, key);
        regime.IncrementRecruits(-recruitCost, key);
        regime.IncrementIndustrialPoints(-industrialCost, key);
        key.Data.Data().Notices.UnitAltered?.Invoke(unit);
    }
}