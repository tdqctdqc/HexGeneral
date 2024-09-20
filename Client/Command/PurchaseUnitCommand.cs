using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game.Client.Command;

public class PurchaseUnitCommand(ERef<Regime> regime, ModelIdRef<UnitModel> model) : GodotUtilities.Server.Command
{
    public ERef<Regime> Regime { get; private set; } = regime;
    public ModelIdRef<UnitModel> Model { get; private set; } = model;

    public override void Handle(LogicKey key)
    {
        var model = Model.Get(key.Data);
        var regime = Regime.Get(key.Data);
        if (model.RecruitCost > regime.Recruits || model.IndustrialCost > regime.IndustrialPoints)
        {
            return;
        }
        var unit = model.Instantiate(regime,
            key.Data.Data());
        var p1 = new EntityCreationProcedure(unit);
        key.SendMessage(p1);
        var p2 = new ResourceCostProcedure(Regime, model.RecruitCost, model.IndustrialCost);
        key.SendMessage(p2);
    }
}