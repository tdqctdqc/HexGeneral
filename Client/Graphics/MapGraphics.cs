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
        AddChild(new HexBaseColorGraphics(Data));
        AddChild(new HexFilterGraphics<Landform>(
            Data, h => h.Landform.Get(Data),
            lf => TextureManager.Textures[lf.Name.ToLower()],
            h => h.GetTerrainColor(Data)));

        var veg = new HexFilterGraphics<Vegetation>(
            Data, h => h.Vegetation.Get(Data),
            v => TextureManager.Textures[v.Name.ToLower()],
            h => Colors.White.Darkened(h.Landform.Get(data).DarkenFactor));
        AddChild(veg);
        veg.Modulate = new Color(1f, 1f, 1f, .5f);
        
        AddChild(new RoadGraphics(Data));
    }

    public Action Disconnect { get; set; }

    public void Process(float delta)
    {
        
    }
}