using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Game.Client;

public class UnitDestroyedEvent(Unit unit, Hex hex) : ClientEvent
{
    public Unit Unit { get; private set; } = unit;
    public Hex Hex { get; private set; } = hex;

    public override void Handle(HexGeneralClient client)
    {
        client.GetComponent<MapGraphics>().Units.RemoveUnit(Unit, Hex, client);
        new HexRedrawEvent(Hex.MakeRef()).Handle(client);
    }
}