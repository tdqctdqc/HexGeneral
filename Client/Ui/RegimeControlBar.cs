using System;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.Ui;
using HexGeneral.Game.Client;

namespace HexGeneral.Client.Ui;

public partial class RegimeControlBar : HBoxContainer, IClientComponent
{
    public Node Node => this;
    private HexGeneralClient _client;
    public void Connect(GameClient client)
    {
        _client = (HexGeneralClient)client;
        client.GetComponent<UiFrame>().TopBars.AddChild(this);
        this.AddButton("Purchase Units", () =>
        {
            PurchaseUnitsWindow.Open(_client);
        });
    }

    public Action Disconnect { get; set; }
    public void Process(float delta)
    {
    }
}