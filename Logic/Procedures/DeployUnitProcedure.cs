using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game;

namespace HexGeneral.Logic.Procedures;

public class DeployUnitProcedure(ERef<Unit> unit, HexRef hex) : GodotUtilities.Server.Procedure
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public HexRef Hex { get; private set; } = hex;

    public override void Handle(ProcedureKey key)
    {
        var data = key.Data.Data();
        var unit = Unit.Get(data);
        data.MapUnitHolder
            .DeployUnit(Unit.Get(data), Hex.Get(data), data);
        data.Notices.UnitDeployed.Invoke(unit);
    }
}