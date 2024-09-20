using System;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.Ui;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Command;

namespace HexGeneral.Client.Ui;

public partial class TurnBar : HBoxContainer, IClientComponent
{
    private HexGeneralClient _client;
    public Node Node => this;
    public void Connect(GameClient client)
    {
        _client = client.Client();
        _client.GetComponent<UiFrame>().TopBars.AddChild(this);
        _client.Data.Notices.TurnStarted.Subscribe(Draw);
        Draw();
    }

    public Action Disconnect { get; set; }
    public void Process(float delta)
    {
        
    }

    private void Draw()
    {
        this.ClearChildren();
        this.CreateLabelAsChild($"Turn: {_client.Data.TurnManager.RoundNumber}");
        AddChild(new VSeparator());
        this.CreateLabelAsChild(
            $"Current Regime: {_client.Data.TurnManager.GetCurrentRegime().Get(_client.Data).RegimeModel.Get(_client.Data).Name}");
        AddChild(new VSeparator());
        this.AddButton("End Turn", () => _client.SubmitCommand(new SubmitTurnCommand()));
    }
    
    // public override void _UnhandledInput(InputEvent e)
    // {
    //     GD.Print($"{GetType().Name} getting unhandled input");
    // }
    // public override void _Input(InputEvent e)
    // {
    //     GD.Print($"{GetType().Name} getting input");
    // }
}