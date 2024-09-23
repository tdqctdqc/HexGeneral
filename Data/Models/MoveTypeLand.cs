using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game.Components;

namespace HexGeneral.Game;

public class MoveTypeLand : MoveType
{
    public override void MoveThru(Hex hex, Unit unit, ProcedureKey key)
    {
        hex.SetRegime(unit.Regime, key);
    }

    public override IMoveComponent MakeNativeMoveComponent(ERef<Unit> unit)
    {
        return new NativeMoveComponent(unit);
    }
}