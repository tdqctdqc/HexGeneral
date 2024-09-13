using System;
using System.Linq;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using GodotUtilities.Ui;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Command;
using HexGeneral.Game.Logic;

namespace HexGeneral.Client.Ui;

public partial class NewGameIntroPanel : PanelContainer
{
    private HexGeneralData _data;

    public NewGameIntroPanel(HexGeneralData data)
    {
        _data = data;
    }

    public override void _Ready()
    {
        Size = new Vector2(200f, 200f);
        var vbox = new VBoxContainer();
        var regimes = _data.Entities.GetAll<Regime>().ToList();
        var regimeScroll = new ItemListToken<Regime>(_data.TurnManager.RegimeOrder.Select(r => r.Get(_data)),
            r => r.RegimeModel.Get(_data).Name, false);
        regimeScroll.ItemList.ExpandFill();
        vbox.AddChild(regimeScroll.ItemList);
        regimeScroll.SelectAt(0);
        
        
        
        vbox.AddButton("Start Game",
            () =>
            {
                var guid = Guid.NewGuid();
                var logic = new HexGeneralHostLogic(_data, guid);
                var client = new HexGeneralClient(logic, _data,
                    guid);
                var parent = GetParent();
                parent.QueueFree();
                this.QueueFree();
                Root.I.AddChild(client);
                Root.I.SetClient(client);
                logic.SetupHostPlayer(regimeScroll.Selected.Single());
                logic.DoFirstTurnStart();
            });
        AddChild(vbox);
        this.Center();
    }
}