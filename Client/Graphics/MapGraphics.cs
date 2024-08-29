using System;
using Godot;
using GodotUtilities.GameClient;

namespace HexGeneral.Game.Client.Graphics;

public partial class MapGraphics(HexGeneralData data) : Node2D, IClientComponent
{
    public Node Node => this;
    public HexGeneralData Data { get; private set; } = data;
    
    public void Connect(GameClient client)
    {
        client.GraphicsLayer.AddChild(this);
        var hexGraphics = new HexGraphics(Data);
        AddChild(hexGraphics);
    }

    public Action Disconnect { get; set; }

    public void Process(float delta)
    {
        
    }
}