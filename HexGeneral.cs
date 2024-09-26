using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using HexGeneral.Game.Client;

namespace HexGeneral.Game;

public static class HexGeneral
{
    public static HexGeneralData Data(this GodotUtilities.GameData.Data data)
    {
        return (HexGeneralData)data;
    }
    public static HexGeneralClient Client(this GameClient client)
    {
        return (HexGeneralClient)client;
    }
}