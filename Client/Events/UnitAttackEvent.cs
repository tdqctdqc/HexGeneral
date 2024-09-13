using HexGeneral.Game.Client.Graphics;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game.Client;

public class UnitAttackEvent(UnitAttackedProcedure procedure) : ClientEvent
{
    private UnitAttackedProcedure _procedure = procedure;

    public override void Handle(HexGeneralClient client)
    {
        var unit = _procedure.Unit.Get(client.Data);
        var targetUnit = _procedure.TargetUnit.Get(client.Data);
        var mapGraphics = client.GetComponent<MapGraphics>();
        mapGraphics.Units.UpdateUnit(unit, client);
        mapGraphics.Units.UpdateUnit(targetUnit, client);
    }
}