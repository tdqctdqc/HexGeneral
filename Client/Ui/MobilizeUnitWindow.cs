using System.Linq;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Ui;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Command;
using HexGeneral.Game.Components;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Client.Ui;

public partial class MobilizeUnitWindow(Unit unit, 
    HexGeneralClient client) : Window
{
    private ItemListToken<Mobilizer> _mobilizers;
    private VBoxContainer _unitInfoContainer, _mobInfoContainer;
    private Unit _unit = unit;

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
        var unitModel = unit.UnitModel.Get(client.Data);
        
        _mobilizers = new ItemListToken<Mobilizer>(
            client.Data.Models.GetModels<Mobilizer>()
                .Where(m => m.AllowedMoveTypes.Contains(unitModel.MoveType)),
            m => m.Name,
            m => m.GetTexture(),
            50, false);
        _mobilizers.SetExpand(5, 200);
        
        left.AddChild(_mobilizers.ItemList);
        
        _mobilizers.ItemList.FullRect();
        _unitInfoContainer = new VBoxContainer();
        _unitInfoContainer.ExpandFill(3);
        box.AddChild(_unitInfoContainer);
        _mobInfoContainer = new VBoxContainer();
        _mobInfoContainer.ExpandFill(3);
        box.AddChild(_mobInfoContainer);
        DrawUnitInfo();
        _mobilizers.JustSelected += DrawMobilizerInfo;
        _mobilizers.SelectAt(0);
    }

    public static MobilizeUnitWindow Open(Unit u, HexGeneralClient client)
    {
        var w = new MobilizeUnitWindow(u, client);
        client.WindowHolder.OpenWindowFullSize(w);
        return w;
    }

    private void DrawUnitInfo()
    {
        
    }
    private void DrawMobilizerInfo()
    {
        _mobInfoContainer.ClearChildren();
        var regime = client.GetPlayer()?.Regime.Get(client.Data);
        if (regime is null) return;
        var model = _mobilizers.Selected.Single();
        _mobInfoContainer.CreateLabelAsChild(model.Name);
        _mobInfoContainer.CreateLabelAsChild($"Recruits: {regime.Recruits}/{model.RecruitCost}");
        _mobInfoContainer.CreateLabelAsChild($"Industrial Points: {regime.IndustrialPoints}/{model.IndustrialCost}");
        var canBuild = regime.Recruits >= model.RecruitCost
                       && regime.IndustrialPoints >= model.IndustrialCost;
        var b = _mobInfoContainer.AddButton("Purchase", () =>
        {
            if (MobilizerComponent.CanAdd(unit, client.Data) == false)
            {
                return;
            }
            var component = new MobilizerComponent(model.MakeIdRef(client.Data),
                false, unit.MakeRef(), null);
            var proc = new AddEntityComponentProcedure<Unit>(unit.MakeRef(), component);
            var inner = new DoProcedureCommand(proc);
            
            var com = CallbackCommand.Redraw(
                inner,
                this, () =>
                {
                    DrawMobilizerInfo();
                    DrawUnitInfo();
                }, client);
            client.SubmitCommand(com);
        });

        var existingMob = unit.EntityComponents
            .Get<MobilizerComponent>()?.Mobilizer.Get(client.Data);
        b.Disabled = canBuild == false || model == existingMob;
    }
}