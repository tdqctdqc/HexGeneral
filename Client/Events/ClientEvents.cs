using System.Collections.Generic;

namespace HexGeneral.Game.Client;

public class ClientEvents
{
    private HexGeneralClient _client;
    public Queue<ClientEvent> QueuedEvents { get; private set; }
    public ClientEvents(HexGeneralClient client)
    {
        _client = client;
        var data = _client.Data;
        QueuedEvents = new Queue<ClientEvent>();
        data.Notices.UnitMoved += p =>
        {
            Enqueue(new UnitMoveEvent(p));
        };
        data.Notices.UnitAttacked += p =>
        {
            Enqueue(new UnitAttackEvent(p));
        };
        data.Notices.UnitDestroyed += (u, h) =>
        {
            Enqueue(new UnitDestroyedEvent(u, h));
        };
        data.Notices.UnitRetreated += p =>
        {
            Enqueue(new UnitRetreatEvent(p));
        };
        data.Notices.FinishedTurnStartLogic += () =>
        {
            Enqueue(new TurnStartEvent());
        };
    }

    private void Enqueue(ClientEvent e)
    {
        var turnManager = _client.Data.TurnManager;
        var curr = turnManager.GetCurrentPlayer(_client.Data);
        if (curr.Guid == _client.PlayerGuid)
        {
            e.Handle(_client);
        }
        else
        {
            QueuedEvents.Enqueue(e);
        }
    }
}