using HexGeneral.Game.Client.Graphics;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game.Client;

public class UnitRetreatEvent(UnitRetreatProcedure procedure) : ClientEvent
{
    private UnitRetreatProcedure _procedure = procedure;

    public override void Handle(HexGeneralClient client)
    {
        var mapGraphics = client.GetComponent<MapGraphics>();
        var unit = _procedure.Unit.Get(client.Data);
        mapGraphics.Units.UpdateUnit(unit, client);
        mapGraphics.Units.DrawHex(_procedure.From, client);
        mapGraphics.Units.DrawHex(_procedure.To, client);
    }
}