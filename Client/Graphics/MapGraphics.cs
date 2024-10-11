using System;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;

namespace HexGeneral.Game.Client.Graphics;

public partial class MapGraphics(HexGeneralClient client) : Node2D, IClientComponent
{
    public Node Node => this;
    public HexGeneralClient Client { get; private set; } = client;
    public UnitGraphics Units { get; private set; }
    public RegimeGraphics RegimeGraphics { get; private set; }
    public RoadGraphics RoadGraphics { get; private set; }
    public LocationGraphics LocationGraphics { get; private set; }
    public ConstructionGraphics ConstructionGraphics { get; private set; }
    public HexBaseColorGraphics TerrainGraphics { get; private set; }
    public MapInputCatcher Input { get; private set; }
    public Action Disconnect { get; set; }

    public void Connect(GameClient client)
    {
        client.GraphicsLayer.AddChild(this);
        client.Client().Data.Notices.FinishedTurnStartLogic.SubscribeForNode(
        () =>
        {
            var curr = Client.Data.TurnManager.GetCurrentPlayer(Client.Data);
            var local = Client.GetPlayer();
            if (curr == local)
            {
                UpdateForTurn();
            }
        }, this);
        Input = new MapInputCatcher(Client.Data.Map.GridBounds);
        AddChild(Input);
        TerrainGraphics = new HexBaseColorGraphics(Client.Data);
        AddChild(TerrainGraphics);
        // AddChild(new HexFilterGraphics<Landform>(
        //     Client.Data, h => h.Landform.Get(Client.Data),
        //     lf => lf.GetTexture(),
        //     h => h.GetTerrainColor(Client.Data)));
        // var veg = new HexFilterGraphics<Vegetation>(
        //     Client.Data, h => h.Vegetation.Get(Client.Data),
        //     v => v.GetTexture(),
        //     h => Colors.White.Darkened(h.Landform.Get(Client.Data).DarkenFactor));
        // AddChild(veg);
        // veg.Modulate = new Color(1f, 1f, 1f, .25f);
        
        
        RegimeGraphics = new RegimeGraphics(Client);
        AddChild(RegimeGraphics);
        
        RoadGraphics = new RoadGraphics(Client.Data);
        AddChild(RoadGraphics);
        
        Units = new UnitGraphics(Client);
        AddChild(Units);

        LocationGraphics = new LocationGraphics(Client);
        AddChild(LocationGraphics);
        ConstructionGraphics = new ConstructionGraphics(Client);
        AddChild(ConstructionGraphics);
    }

    public void UpdateForTurn()
    {
        Units.Update(Client);
        RegimeGraphics.Update(Client);
        RoadGraphics.Update();
        LocationGraphics.Update();
        ConstructionGraphics.Update(Client);
    }

    public void RedrawHex(HexRef h, HexGeneralClient client)
    {
        Units.DrawHex(h, client);
        RegimeGraphics.UpdateHex(h.Get(client.Data), client);
        if (h.Get(client.Data).TryGetLocation(client.Data, out var loc))
        {
            LocationGraphics.UpdateLocation(loc);
        }
        ConstructionGraphics.Update(client);
        TerrainGraphics.UpdateHex(h.Get(client.Data));
    }

    public void Process(float delta)
    {
        
    }
}