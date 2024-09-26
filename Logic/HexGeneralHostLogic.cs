using System;
using System.Linq;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game.Procedures;

namespace HexGeneral.Game.Logic;

public class HexGeneralHostLogic : HostLogic
{
    public HexGeneralHostLogic(GodotUtilities.GameData.Data data, Guid hostPlayerGuid) : base(data, hostPlayerGuid)
    {
        HostLogicData = new HostLogicData();
        data.Data().Notices.FinishedTurnStartLogic.Subscribe(HandleAi);
    }

    public HostLogicData HostLogicData { get; private set; }
    
    public void SetupHostPlayer(Regime regime)
    {
        var player = new Player(_data.IdDispenser.TakeId(),
            new ERef<Regime>(), HostPlayerGuid);
        _logicKey.SendMessage(new EntityCreationProcedure(player));
        _logicKey.SendMessage(new SetPlayerRegimeProcedure(HostPlayerGuid, regime.MakeRef()));
    }

    public void DoFirstTurnStart()
    {
        _logicKey.SendMessage(new StartTurnProcedure());
        
    }

    private void HandleAi()
    {
        var turnManager = _data.Data().TurnManager;
        var currPlayer = turnManager.GetCurrentPlayer(_data.Data());
        if (currPlayer is not null) return;
        
        HostLogicData.HandleAi(_data.Data());
        _logicKey.SendMessage(new EndTurnProcedure());
    }
    public override void Process(double delta)
    {
        var turnManager = _data.Data().TurnManager;
        var currPlayer = turnManager.GetCurrentPlayer(_data.Data());
        if (currPlayer is null) return;
        
        while (turnManager.AcceptingOrders
               && CommandQueue.TryDequeue(out var com))
        {
            if (com.CommandingPlayerGuid != currPlayer.Guid) continue;
            com.Handle(_logicKey);
        }
    }
}