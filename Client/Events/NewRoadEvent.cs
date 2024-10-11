using Godot;
using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Game.Client;

public class NewRoadEvent(Vector2I edge) : ClientEvent
{
    public Vector2I Edge { get; private set; } = edge;
    
    public override void Handle(HexGeneralClient client)
    {
        client.GetComponent<MapGraphics>().RoadGraphics.Update();
    }
}