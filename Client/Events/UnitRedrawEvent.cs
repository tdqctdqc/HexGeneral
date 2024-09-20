using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Game.Client;

public class UnitRedrawEvent(Unit unit) : ClientEvent
{
    public Unit Unit { get; private set; } = unit;

    public override void Handle(HexGeneralClient client)
    {
        client.GetComponent<MapGraphics>().Units
            .UpdateUnit(Unit, client);
    }
}