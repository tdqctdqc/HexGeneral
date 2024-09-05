using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Client.Ui;

namespace HexGeneral.Game.Client;

public class UnitMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;
    private HexGeneralClient _hexGenClient;
    private MouseMode _mouseMode;
    public Action Exited { get; set; }
    public DefaultSettingsOption<Unit> SelectedUnit { get; private set; }
    public UnitMode(HexGeneralClient client, string name) : base(client, name)
    {
        SelectedUnit = new DefaultSettingsOption<Unit>("Selected Unit",
            null);
        _hexGenClient = client;
        _mouseOverHandler = new MouseOverHandler();
        _mouseMode = new MouseMode(
            new List<MouseAction>
            {
                new UnitSelectAction(MouseButtonMask.Left,
                    _hexGenClient, _mouseOverHandler, SelectedUnit.Set)
            }
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
        Exited?.Invoke();
    }

    public override Control GetControl(GameClient client)
    {
        return new UnitPanel(this, (HexGeneralClient)client);
    }
}