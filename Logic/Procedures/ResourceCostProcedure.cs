using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game;

namespace HexGeneral.Logic.Procedures;

public class ResourceCostProcedure(ERef<Regime> regime, float recruitCost, float industrialCost)
    : GodotUtilities.Server.Procedure
{
    public ERef<Regime> Regime { get; private set; } = regime;
    public float RecruitCost { get; private set; } = recruitCost;
    public float IndustrialCost { get; private set; } = industrialCost;

    public override void Handle(ProcedureKey key)
    {
        var r = Regime.Get(key.Data);
        r.IncrementRecruits(-RecruitCost, key);
        r.IncrementIndustrialPoints(-IndustrialCost, key);
    }
}