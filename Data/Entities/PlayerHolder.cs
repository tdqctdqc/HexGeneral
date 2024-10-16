using System;
using System.Collections.Generic;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class PlayerHolder : Entity, ISingletonEntity
{
    public Dictionary<Guid, ERef<Player>> PlayerByGuid { get; private set; }
    
    public PlayerHolder(int id, Dictionary<Guid, ERef<Player>> playerByGuid) : base(id)
    {
        PlayerByGuid = playerByGuid;
    }

    public override void Made(GodotUtilities.GameData.Data d)
    {
    }

    public override void CleanUp(GodotUtilities.GameData.Data d)
    {
        throw new System.Exception();
    }
}