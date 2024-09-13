namespace HexGeneral.Game.Client.Command;

public static class CommandExt
{
    public static Player GetCommandingPlayer(this GodotUtilities.Server.Command c,
        HexGeneralData d)
    {
        return d.PlayerHolder.PlayerByGuid[c.CommandingPlayerGuid].Get(d);
    }
}