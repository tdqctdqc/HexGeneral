using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Client.Ui;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Game.Components;

namespace HexGeneral.Game.Client;

public class DeploymentMode : UiMode
{
    public DefaultSettingsOption<Unit> SelectedUnit { get; private set; }
    private MapOverlayDrawer _deploySpots;
    public DeploymentMode(HexGeneralClient client) 
        : base(client, "Deployment")
    {
        SelectedUnit = new DefaultSettingsOption<Unit>("Selected Unit", null);
        
        var deployMouseMode = new MouseMode(
            new List<MouseAction>
            {
                new DeployUnitAction(MouseButtonMask.Right, 
                    this, client)
            });
        MouseMode.Set(deployMouseMode);
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
        
        SelectedUnit.SettingChanged.Subscribe(v =>
        {
            DrawDeploySpots();
        });
        DrawDeploySpots();
    }

    public override void Process(float delta)
    {
    }

    public override void HandleInput(InputEvent e)
    {
        if (e is InputEventMouse m)
        {
            MouseMode.Value?.HandleInput(m);
        }
    }

    private void DrawDeploySpots()
    {
        _deploySpots.Clear();
        var data = _client.Client().Data;

        var clientPlayer = _client.Client().GetPlayer();
        if (clientPlayer is null) return;

        var regime = clientPlayer.Regime.Get(data);
        var unit = SelectedUnit.Value;
        if (unit is null) return;
        var unitDomain = unit.Components.Get<IMoveComponent>(data)
            .GetActiveMoveType(data).Domain;

        _deploySpots.Draw(mb =>
        {
            var deployableHexes = regime.Hexes
                .Select(c => data.Map.Hexes[c])
                .Where(h => h.Regime == regime
                            && h.CanDeploy(unitDomain, data));
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