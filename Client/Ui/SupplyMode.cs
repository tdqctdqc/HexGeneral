using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.PathFinder;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Ui;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Game.Logic;

namespace HexGeneral.Game.Client;

public class SupplyMode : UiMode
{
    public SettingsOption<Hex> SelectedHex { get; private set; }
    public SettingsOption<Regime> SelectedRegime { get; private set; }
    private MouseMode _mouseMode;
    private MapOverlayDrawer _supplyOverlay;
    private static float _tint = 1f;

    public SupplyMode(HexGeneralClient client) : base(client, "Supply Mode")
    {
        SelectedHex = new DefaultSettingsOption<Hex>("Selected Hex", null);
        SelectedRegime = new DefaultSettingsOption<Regime>("Selected Regime", null);
        _supplyOverlay = new MapOverlayDrawer((int)GraphicsLayers.SubRoads,
            client.GetComponent<MapGraphics>);
        _mouseMode = new MouseMode(
            new List<MouseAction>
            {
                new HexSelectAction(MouseButtonMask.Left, 
                    client, 
                    SelectedHex.Set)
            });
        
        SelectedHex.SettingChanged.Subscribe(v =>
        {
            // DrawSupplyFromHex(client, v.newVal);
            if (v.newVal is null
                || v.newVal.Regime.IsEmpty())
            {
                SelectedRegime.Set(null);
            }
            else
            {
                SelectedRegime.Set(v.newVal.Regime.Get(client.Data));
            }
        });
        
        SelectedRegime.SettingChanged.Subscribe(v =>
        {
            if (v.newVal is Regime r == false) return;
            DrawSupplyForRegime(r, client);
        });
    }

    private void DrawSupplyFromHex(HexGeneralClient client, 
        Hex hex)
    {
        _supplyOverlay.Clear();
        var supplyMove = client.Data.ModelPredefs.MoveTypes.SupplyMoveType;
        if (hex is null || hex.Regime.Fulfilled() == false) return;
        var regime = hex.Regime.Get(client.Data);
        var flood = PathFinder<Hex>.FloodFillGetCosts(
            hex, h => h.GetNeighbors(client.Data)
                .Where(n => n.Regime == hex.Regime),
            (h,g) => supplyMove.GetMoveCost(h, g, regime, client.Data),
            SupplyLogic.HalfSupplyRange);
        var hexTris = ShapeBuilder
            .GetHex(Vector2.Zero, 1f).ToArray();

        _supplyOverlay.Draw(mb =>
        {
            foreach (var (h, cost) in flood)
            {
                var score = 1f - cost / SupplyLogic.HalfSupplyRange;
                score = Mathf.Clamp(score, 0f, 1f);
                var color = ColorsExt.GetHealthColor(score).Tint(_tint);
                mb.AddTris(hexTris.Select(v => v + h.WorldPos()), color);
            }
        }, Vector2.Zero);
    }
    
    private void DrawSupplyForRegime(Regime regime, HexGeneralClient client)
    {
        _supplyOverlay.Clear();
        var supplyMove = client.Data.ModelPredefs.MoveTypes.SupplyMoveType;

        var sources = regime.Hexes.Select(h => client.Data.Map.Hexes[h])
            .Where(h => h.TryGetLocation(client.Data, out var l)
                && l.Buildings.Any(m => m.Get(client.Data) is ISupplyCenter))
            .ToList();

        var flood = FloodFill<Hex>.FindMinDistMap(
            sources, (h, g) => supplyMove.GetMoveCost(h, g, regime, client.Data),
            h => h.GetNeighbors(client.Data).Where(n => n.Regime == regime)
        );
        
            
            
        var hexTris = ShapeBuilder
            .GetHex(Vector2.Zero, 1f).ToArray();

        _supplyOverlay.Draw(mb =>
        {
            foreach (var (h, cost) in flood)
            {
                var score = SupplyLogic.GetSupplyAvailabilityFromMoveCost(cost);
                var color = ColorsExt.GetHealthColor(score).Tint(_tint);
                mb.AddTris(hexTris.Select(v => v + h.WorldPos()), color);
            }
        }, Vector2.Zero);
    }

    public override void Process(float delta)
    {
        
    }

    public override void HandleInput(InputEvent e)
    {
        if (e is InputEventMouse m)
        {
            _mouseMode.HandleInput(m);
        }
    }

    public override void Enter()
    {
        
    }

    public override void Clear()
    {
        _supplyOverlay.Clear();
    }

    public override Control GetControl(GameClient client)
    {
        return new Control();
    }
}