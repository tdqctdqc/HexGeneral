using System.Collections.Generic;
using Godot;

namespace HexGeneral.Game.Client.Graphics;

public partial class LocationGraphics : Node2D
{
    public Dictionary<HexRef, HexLocationGraphic> Graphics { get; private set; }
    private HexGeneralClient _client;
    public LocationGraphics(HexGeneralClient client)
    {
        _client = client;
        ZIndex = (int)GraphicsLayers.Locations;
        ZAsRelative = false;
        Graphics = new Dictionary<HexRef, HexLocationGraphic>();
        Update();
    }

    public void Update()
    {
        this.ClearChildren();
        Graphics.Clear();
        foreach (var location in _client.Data.Entities.GetAll<Location>())
        {
            UpdateLocation(location);
        }
    }

    public void UpdateLocation(Location location)
    {
        var data = _client.Data;
        var hex = location.Hex.Get(data);
        if (Graphics.TryGetValue(hex.MakeRef(), out var graphic)
            == false)
        {
            graphic = new HexLocationGraphic();
            Graphics.Add(hex.MakeRef(), graphic);
            graphic.Position = hex.WorldPos();
            AddChild(graphic);
        }
        graphic.DrawLocation(location, _client);
    }
}