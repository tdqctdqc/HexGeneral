using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Game.Client;

public class TurnStartEvent : ClientEvent
{
    public override void Handle(HexGeneralClient client)
    {
        var mapGraphics = client.GetComponent<MapGraphics>();
        mapGraphics.Units.Update(client);
    }
}