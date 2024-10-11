using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Client.Ui;
using HexGeneral.Data.Components;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Game.Components;

namespace HexGeneral.Game.Client;

public class UnitMode : UiMode
{
    private HexGeneralClient _hexGenClient;
    public MouseMode MoveAttackMouseMode { get; private set; }
    public MouseMode EngineerMouseMode { get; private set; }
    public MouseMode SelectUnitMouseMode { get; private set; }
    private KeyboardInputMode _keyboardMode;
    private List<MapOverlayDrawer> _overlays;
    public Action Exited { get; set; }
    public DefaultSettingsOption<Unit> SelectedUnit { get; private set; }
    public UnitMode(HexGeneralClient client, string name) : base(client, name)
    {
        SelectedUnit = new DefaultSettingsOption<Unit>("Selected Unit",
            null);
        _hexGenClient = client;

        var selectedOverlay = new MapOverlayDrawer((int)GraphicsLayers.Debug, _client.GetComponent<MapGraphics>);
        SelectedUnit.SettingChanged.Subscribe(v =>
        {
            selectedOverlay.Clear();
            var unit = v.newVal;
            if (unit is null)
            {
                MouseMode.Set(SelectUnitMouseMode);
                return;
            }
            selectedOverlay.DrawUnitHighlightBox(unit, Colors.White,
                client);

            if (MouseMode.Value == EngineerMouseMode
                && unit.Components.Get<EngineerEntityComponent>(client.Data) is null)
            {
                MouseMode.Set(MoveAttackMouseMode);
            }

            if (MouseMode.Value == SelectUnitMouseMode)
            {
                MouseMode.Set(MoveAttackMouseMode);
            }
        });
        
        _overlays = new List<MapOverlayDrawer>
        {
            selectedOverlay
        };

        SelectUnitMouseMode = new MouseMode(
            [
                new UnitSelectAction(MouseButtonMask.Left, _hexGenClient, SelectedUnit.Set),
            ]
        );
        
        MoveAttackMouseMode = new MouseMode(
            [
                new UnitSelectAction(MouseButtonMask.Left, _hexGenClient, SelectedUnit.Set),
                new UnitMoveAction(SelectedUnit, _hexGenClient, MouseButtonMask.Right),
                new UnitAttackAction(SelectedUnit, _hexGenClient, MouseButtonMask.Right),
                new EmbarkShipAction(SelectedUnit, _hexGenClient, MouseButtonMask.Right),
                new DisembarkShipAction(SelectedUnit, _hexGenClient, MouseButtonMask.Right),
            ]
        );

        EngineerMouseMode = new MouseMode(
            [
                new UnitSelectAction(MouseButtonMask.Left, _hexGenClient, SelectedUnit.Set),
                new UnitBuildRoadAction(MouseButtonMask.Right, SelectedUnit, _hexGenClient),
                new UnitBuildPortAction(MouseButtonMask.Right, SelectedUnit, _hexGenClient),
            ]
        );
        
        
        MouseMode.SettingChanged.Subscribe(v =>
        {
            if (v.oldVal != v.newVal)
            {
                v.oldVal?.Clear();
            }
        });
        MouseMode.Set(SelectUnitMouseMode);
        
        _keyboardMode = new UnitKeyboardMode(client);
    }

    public override void Process(float delta)
    {
        
    }

    public override void HandleInput(InputEvent e)
    {
        if (e is InputEventMouse mb)
        {
            MouseMode.Value?.HandleInput(mb);
        }
        if (e is InputEventKey k)
        {
            _keyboardMode.Handle(k);
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
        MouseMode.Value?.Clear();
        Exited?.Invoke();
    }

    public override Control GetControl(GameClient client)
    {
        return new UnitPanel(this, client.Client());
    }
}