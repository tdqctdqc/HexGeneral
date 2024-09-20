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
        var unit = Unit.Get(key.Data);
        key.Data.Data().MapUnitHolder
            .DeployUnit(Unit.Get(key.Data), Hex.Get(key.Data));
        key.Data.Data().Notices.UnitDeployed.Invoke(unit);
    }
}