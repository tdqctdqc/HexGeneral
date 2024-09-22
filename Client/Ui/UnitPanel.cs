using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Ui;
using HexGeneral.Client.Ui;
using HexGeneral.Game.Components;
using HexGeneral.Game.Logic;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game.Client;

public partial class UnitPanel : PanelContainer
{
    private UnitMode _mode;
    private HexGeneralClient _client;
    public UnitPanel(UnitMode mode, HexGeneralClient client)
    {
        _client = client;
        _mode = mode;
        _mode.SelectedUnit.SettingChanged.SubscribeForNode(v =>
        {
            Draw();
        }, this);
        TreeEntered += Draw;
        VisibilityChanged += () =>
        {
            if (Visible) Draw();
        };
        _client.Data.Notices.UnitAltered.SubscribeForNode(u =>
        {
            Draw();
        }, this);
        _client.Data.Notices.UnitMoved.SubscribeForNode(u =>
        {
            Draw();
        }, this);
        _client.Data.Notices.FinishedTurnStartLogic.SubscribeForNode(Draw, this);
    }




    private void Draw()
    {
        this.ClearChildren();
        var vbox = new VBoxContainer();
        AddChild(vbox);
        var unit = _mode.SelectedUnit.Value;
        if (unit is not null)
        {
            DrawUnitInfo(unit, vbox);
        }
    }

    private void DrawUnitInfo(Unit unit, VBoxContainer vbox)
    {
        var regime = unit.Regime.Get(_client.Data);
        var model = unit.UnitModel.Get(_client.Data);
        var texture = new TextureRect();
        texture.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
        texture.Texture = model.GetTexture();
        texture.Size = Vector2.One * 20f;
        texture.CustomMinimumSize = texture.Size;
        vbox.AddChild(texture);
        vbox.CreateLabelAsChild(model.Name);
        vbox.CreateLabelAsChild($"Hitpoints: {unit.CurrentHitPoints} / {model.HitPoints}");
        vbox.CreateLabelAsChild($"Organization: {unit.CurrentOrganization} / {model.Organization}");
        vbox.CreateLabelAsChild($"Ammunition: {unit.CurrentAmmo} / {model.AmmoCap}");
        vbox.AddChild(new VSeparator());
        var effective = CombatLogic.GetEffectiveAttackRatio(unit, _client.Data);
        vbox.CreateLabelAsChild($"Soft Attack: {model.SoftAttack * effective} / {model.SoftAttack}");
        vbox.CreateLabelAsChild($"Hard Attack: {model.HardAttack * effective} / {model.HardAttack}");
        vbox.CreateLabelAsChild($"Hardness: {model.Hardness}");
        vbox.CreateLabelAsChild($"Range: {model.Range}");

        var hex = unit.GetHex(_client.Data);
        var supplyAvailability = SupplyLogic.GetSupplyAvailability(hex,
            _client.Data);
        vbox.CreateLabelAsChild($"Supply Availability: {supplyAvailability}");
        
        
        foreach (var unitComponent in unit.EntityComponents.Components)
        {
            vbox.AddChild(unitComponent.GetDisplay(_client));
        }
        
        
        var reinforce = vbox.AddButton($"Reinforce", () =>
        {
            var missingRatio = 1f - unit.CurrentHitPoints / model.HitPoints;
            if (missingRatio == 0f) return;
            var industrialRatioCap = Mathf.Clamp(
                regime.IndustrialPoints / model.IndustrialCost,
                0f, 1f);
            var recruitRatioCap = Mathf.Clamp(
                regime.Recruits / model.RecruitCost,
                0f, 1f);
            var ratioCap = Mathf.Min(missingRatio, industrialRatioCap);
            ratioCap = Mathf.Min(ratioCap, recruitRatioCap);
            ratioCap = Mathf.Min(ratioCap, supplyAvailability);
            if (ratioCap == 0f) return;
            var amount = ratioCap * model.HitPoints;
            var proc = new UnitReinforceProcedure(unit.MakeRef(), amount);
            var inner = new DoProcedureCommand(proc);
            var com = CallbackCommand.Redraw(inner, this,
                Draw, _client);
            _client.SubmitCommand(com);
        });
        reinforce.Disabled = unit.CanReinforce() == false;
        
        var resupply = vbox.AddButton($"Resupply Ammo", () =>
        {
            var missingAmount = model.AmmoCap - unit.CurrentAmmo;

            if (missingAmount == 0) return;

            var industrialCap = Mathf.FloorToInt(regime.IndustrialPoints / model.AmmoCost);
            var cap = Mathf.Min(missingAmount, industrialCap);
            cap = Mathf.Min(cap, Mathf.FloorToInt(model.AmmoCap * supplyAvailability));
            if (cap == 0) return;
            var proc = new UnitRestockAmmoProcedure(unit.MakeRef(), cap);
            var inner = new DoProcedureCommand(proc);
            
            var com = CallbackCommand.Redraw(inner, this, Draw, _client);
            _client.SubmitCommand(com);
        });
        resupply.Disabled = unit.CanRestockAmmo() == false;


        var mobilize = vbox.AddButton("Mobilize", () =>
        {
            MobilizeUnitWindow.Open(unit, _client);
        });
        mobilize.Disabled = MobilizerComponent.CanAdd(unit, _client.Data) == false;
    }
}