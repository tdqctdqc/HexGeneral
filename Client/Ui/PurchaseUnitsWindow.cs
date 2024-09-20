using System.Linq;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Ui;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Command;

namespace HexGeneral.Client.Ui;

public partial class PurchaseUnitsWindow : Window
{
    private ItemListToken<UnitModel> _units;
    private HexGeneralClient _client;
    public PurchaseUnitsWindow(HexGeneralClient client)
    {
        _client = client;
    }

    public override void _Ready()
    {
        this.MakeFreeable();

        var box = new HBoxContainer();
        box.ExpandFill();
        box.FullRect();
        AddChild(box);
        var left = new VBoxContainer();
        left.ExpandFill(1);
        box.AddChild(left);
        
        _units = new ItemListToken<UnitModel>(
            _client.Data.Models.GetModels<UnitModel>(),
            m => m.Name,
            m => m.GetTexture(),
            50, false);
        _units.SetExpand(5, 200);
        
        left.AddChild(_units.ItemList);
        
        _units.ItemList.FullRect();
        var unitInfoContainer = new VBoxContainer();
        unitInfoContainer.ExpandFill(3);
        box.AddChild(unitInfoContainer);
        _units.JustSelected += () => DrawUnitModelInfo(unitInfoContainer);
        _units.SelectAt(0);
    }

    public static void Open(HexGeneralClient client)
    {
        var w = new PurchaseUnitsWindow(client);

        client.WindowHolder.OpenWindowFullSize(w);
    }

    private void DrawUnitModelInfo(VBoxContainer container)
    {
        container.ClearChildren();
        var regime = _client.GetPlayer()?.Regime.Get(_client.Data);
        if (regime is null) return;
        var model = _units.Selected.Single();
        container.CreateLabelAsChild(model.Name);
        container.CreateLabelAsChild($"Recruits: {regime.Recruits}/{model.RecruitCost}");
        container.CreateLabelAsChild($"Industrial Points: {regime.IndustrialPoints}/{model.IndustrialCost}");
        var canBuild = regime.Recruits >= model.RecruitCost
                       && regime.IndustrialPoints >= model.IndustrialCost;
        var b = container.AddButton("Purchase", () =>
        {
            var com = CallbackCommand.Redraw(
                new PurchaseUnitCommand(regime.MakeRef(),
                    model.MakeIdRef(_client.Data)),
                this, () => DrawUnitModelInfo(container), _client);
            _client.SubmitCommand(com);
        });
        b.Disabled = canBuild == false;
    }
}