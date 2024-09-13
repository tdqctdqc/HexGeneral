using System;
using System.Linq;
using GodotUtilities.DataStructures;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using GodotUtilities.Server;

namespace HexGeneral.Game.Procedures;

public class SetPlayerRegimeProcedure(Guid playerGuid, ERef<Regime> regime) : Procedure
{
    public Guid PlayerGuid { get; private set; } = playerGuid;
    public ERef<Regime> Regime { get; private set; } = regime;

    public override void Handle(ProcedureKey key)
    {
        var playerHolder = key.Data.Data().PlayerHolder;
        var oldPlayer = key.Data.Entities.GetAll<Player>()
            .FirstOrDefault(p => p.Regime == Regime);
        if (oldPlayer is not null) throw new Exception();
        
        var player = playerHolder
            .PlayerByGuid[PlayerGuid].Get(key.Data);
        player.SetRegime(Regime, key);
        
        key.Data.Data().Notices.PlayerChangedRegime?.Invoke(player);
    }
}