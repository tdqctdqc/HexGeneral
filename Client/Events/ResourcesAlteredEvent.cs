using HexGeneral.Client.Ui;

namespace HexGeneral.Game.Client;

public class ResourcesAlteredEvent(Regime regime) : ClientEvent
{
    public Regime Regime { get; private set; } = regime;
    public override void Handle(HexGeneralClient client)
    {
        client.GetComponent<RegimeInfoBar>().Draw();
    }
}