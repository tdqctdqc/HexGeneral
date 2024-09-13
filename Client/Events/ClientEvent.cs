namespace HexGeneral.Game.Client;

public abstract class ClientEvent
{
    public abstract void Handle(HexGeneralClient client);
}