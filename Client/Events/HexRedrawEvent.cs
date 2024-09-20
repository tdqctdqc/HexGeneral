using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Game.Client;
 
public class HexRedrawEvent(HexRef hex) : ClientEvent
{
    private HexRef _hex = hex;

    public override void Handle(HexGeneralClient client)
    {
        client.GetComponent<MapGraphics>().RedrawHex(_hex, client);
    }
}