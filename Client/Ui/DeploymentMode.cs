using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Client.Ui;
using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Game.Client;

public class DeploymentMode : UiMode
{
    public DefaultSettingsOption<Unit> SelectedUnit { get; private set; }
    private MapOverlayDrawer _deploySpots;
    private MouseMode _mouseMode;
    public DeploymentMode(HexGeneralClient client) 
        : base(client, "Deployment")
    {
        SelectedUnit = new DefaultSettingsOption<Unit>("Selected Unit", null);
        
        _mouseMode = new MouseMode(
            new List<MouseAction>
            {
                new DeployUnitAction(MouseButtonMask.Right, 
                    this, client)
            });
        _deploySpots = new MapOverlayDrawer((int)GraphicsLayers.Debug,
            client.GetComponent<MapGraphics>);
        _client.Client().Data.Notices.UnitDeployed.Subscribe(u =>
        {
            DrawDeploySpots();
            if (u == SelectedUnit.Value)
            {
                SelectedUnit.Set(null);
            }
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

    private void DrawDeploySpots()
    {
        _deploySpots.Clear();
        var clientPlayer = _client.Client().GetPlayer();
        if (clientPlayer is null) return;

        var regime = clientPlayer.Regime.Get(_client.Client().Data);


        _deploySpots.Draw(mb =>
        {
            var deployableHexes = regime.Hexes
                .Select(c => _client.Client().Data.Map.Hexes[c])
                .Where(h => h.Regime == regime
                            && h.CanDeploy(_client.Client().Data));
            foreach (var deployableHex in deployableHexes)
            {
                mb.DrawHex(deployableHex.WorldPos(),
                    1f, Colors.White.Tint(.5f));
            }
        }, Vector2.Zero);
    }
    public override void Enter()
    {
        DrawDeploySpots();
    }

    public override void Clear()
    {
        _deploySpots.Clear();
    }

    public override Control GetControl(GameClient client)
    {
        return new DeploymentPanel(this, (HexGeneralClient)client);
    }
}