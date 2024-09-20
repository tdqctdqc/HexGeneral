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
        data.Notices.UnitMoved.Subscribe(p =>
        {
            Enqueue(new UnitMoveEvent(p));
        });
        data.Notices.UnitDestroyed += (u, h) =>
        {
            Enqueue(new UnitDestroyedEvent(u, h));
        };
        data.Notices.UnitRetreated += p =>
        {
            Enqueue(new HexRedrawEvent(p.From));
            Enqueue(new HexRedrawEvent(p.To));
        };
        data.Notices.FinishedTurnStartLogic.Subscribe(() =>
        {
            Enqueue(new TurnStartEvent());
        });
        data.Notices.UnitAltered.Subscribe(u =>
        {
            Enqueue(new UnitRedrawEvent(u)); 
        });
        data.Notices.UnitDeployed.Subscribe(u =>
        {
            Enqueue(new HexRedrawEvent(u.GetHex(data).MakeRef()));
        });
        data.Notices.ResourcesAltered += r =>
        {
            Enqueue(new ResourcesAlteredEvent(r));
        };
    }

    private void Enqueue(ClientEvent e)
    {
        var turnManager = _client.Data.TurnManager;
        var curr = turnManager.GetCurrentPlayer(_client.Data);
        if (curr is not null && curr.Guid == _client.PlayerGuid)
        {
            e.Handle(_client);
        }
        else
        {
            QueuedEvents.Enqueue(e);
        }
    }
}