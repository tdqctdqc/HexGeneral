using System;
using Godot;
using GodotUtilities.GameClient;
using HexGeneral.Game;

namespace HexGeneral.Client.Ui;

public partial class RegimeInfoBar : HBoxContainer, IClientComponent
{
    private GameClient _client;
    public Node Node => this;
    public void Connect(GameClient client)
    {
        _client = client;
        var data = _client.Client().Data;
        client.GetComponent<UiFrame>().TopBars.AddChild(this);
        data.Notices.PlayerChangedRegime += p =>
        {
            if (p.Guid == _client.PlayerGuid)
            {
                Draw();
            }
        };

        data.Notices.FinishedTurnStartLogic.SubscribeForNode(Draw, this);
    }

    public Action Disconnect { get; set; }

    public void Draw()
    {
        this.ClearChildren();
        var data = _client.Client().Data;
        var player = data.PlayerHolder
            .PlayerByGuid[_client.PlayerGuid].Get(data);
        var r = player.Regime.Get(data);
        if (r is null) return;
        this.CreateLabelAsChild(r.RegimeModel.Get(data).Name);
        this.AddChild(new VSeparator());
        this.CreateLabelAsChild($"Recruits: {r.Recruits}");
        this.AddChild(new VSeparator());
        this.CreateLabelAsChild($"Industrial Points: {r.IndustrialPoints}");
    }
    
    public void Process(float delta)
    {
        
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