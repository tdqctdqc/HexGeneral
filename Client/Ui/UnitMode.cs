using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Client.Ui;
using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Game.Client;

public class UnitMode : UiMode
{
    private HexGeneralClient _hexGenClient;
    private MouseMode _mouseMode;
    private List<MapOverlayDrawer> _overlays;
    public Action Exited { get; set; }
    public DefaultSettingsOption<Unit> SelectedUnit { get; private set; }
    public UnitMode(HexGeneralClient client, string name) : base(client, name)
    {
        SelectedUnit = new DefaultSettingsOption<Unit>("Selected Unit",
            null);
        _hexGenClient = client;

        var pathOverlay = new MapOverlayDrawer((int)GraphicsLayers.Debug, _client.GetComponent<MapGraphics>);
        var damageOverlay = new MapOverlayDrawer((int)GraphicsLayers.Debug, _client.GetComponent<MapGraphics>);
        var radiusOverlay = new MapOverlayDrawer((int)GraphicsLayers.Debug, _client.GetComponent<MapGraphics>);
        var attackPathOverlay = new MapOverlayDrawer((int)GraphicsLayers.Debug, _client.GetComponent<MapGraphics>);
        _overlays = new List<MapOverlayDrawer>
        {
            pathOverlay, radiusOverlay, attackPathOverlay, damageOverlay
        };
        
        _mouseMode = new MouseMode(
            [
                new UnitSelectAction(MouseButtonMask.Left,
                    _hexGenClient, SelectedUnit.Set),

                new UnitMoveAction(radiusOverlay, pathOverlay,
                    SelectedUnit, _hexGenClient, MouseButtonMask.Right),
                
                new UnitAttackAction(MouseButtonMask.Right,
                    SelectedUnit,
                    attackPathOverlay,
                    damageOverlay,
                    _hexGenClient)
            ]
        );

    }

    public override void Process(float delta)
    {
        
    }

    public override void HandleInput(InputEvent e)
    {
        if (e is InputEventMouse mb)
        {
            _mouseMode.HandleInput(mb);
        }
    }

    public override void Enter()
    {
        
    }

    public override void Clear()
    {
        foreach (var overlay in _overlays)
        {
            overlay.Clear();
        }

        Exited?.Invoke();
    }

    public override Control GetControl(GameClient client)
    {
        return new UnitPanel(this, client.Client());
    }
}