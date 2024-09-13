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
    private MouseMode _mouseMode;
    private MapOverlayDrawer _supplyOverlay;

    public SupplyMode(HexGeneralClient client) : base(client, "Supply Mode")
    {
        SelectedHex = new DefaultSettingsOption<Hex>("Selected Hex", null);
        _supplyOverlay = new MapOverlayDrawer(0, client.GetComponent<MapGraphics>);
        _mouseMode = new MouseMode(
            new List<MouseAction>
            {
                new HexSelectAction(MouseButtonMask.Left, 
                    client, 
                    SelectedHex.Set)
            });
        
        SelectedHex.SettingChanged.Subscribe(v =>
        {
            _supplyOverlay.Clear();
            var supplyMove = client.Data.ModelPredefs.MoveTypes.SupplyMoveType;
            var hex = v.newVal;
            if (hex is null || hex.Regime.Fulfilled() == false) return;
            var regime = hex.Regime.Get(client.Data);
            var flood = PathFinder<Hex>.FloodFillGetCosts(
                hex, h => h.GetNeighbors(client.Data)
                    .Where(n => n.Regime == hex.Regime),
                (h,g) => supplyMove.GetMoveCost(h, g, regime, client.Data),
                SupplyLogic.SupplyRange);
            var hexTris = ShapeBuilder.GetHex(Vector2.Zero, 1f);

            _supplyOverlay.Draw(mb =>
            {
                foreach (var (h, cost) in flood)
                {
                    var score = 1f - cost / SupplyLogic.SupplyRange;
                    score = Mathf.Clamp(score, 0f, 1f);
                    var color = ColorsExt.GetHealthColor(score);
                    mb.AddTris(hexTris.Select(v => v + h.WorldPos()), color);
                }
            }, Vector2.Zero);
        });
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