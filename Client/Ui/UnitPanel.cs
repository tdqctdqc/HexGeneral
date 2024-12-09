using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Ui;
using HexGeneral.Client.Ui;
using HexGeneral.Data.Components;
using HexGeneral.Game.Components;
using HexGeneral.Game.Logic;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game.Client;

public partial class UnitPanel : UiModeTabPanel
{
    private UnitMode _mode;
    private HexGeneralClient _client;
    private Container _combatInfo, _engineeringInfo;
    
    public UnitPanel(UnitMode mode, HexGeneralClient client)
        : base(mode, mode.SelectUnitMouseMode)
    {
        _client = client;
        _mode = mode;
        _mode.SelectedUnit.SettingChanged.SubscribeForNode(v =>
        {
            DrawTabs();
        }, this);
        
        _client.Data.Notices.UnitAltered.SubscribeForNode(u =>
        {
            DrawTabs();
        }, this);
        _client.Data.Notices.UnitMoved.SubscribeForNode(u =>
        {
            DrawTabs();
        }, this);
        _client.Data.Notices.FinishedTurnStartLogic.SubscribeForNode(DrawTabs, this);

        _combatInfo = new ScrollContainer();
        _combatInfo.Name = "Combat";

        _engineeringInfo = new ScrollContainer();
        _engineeringInfo.Name = "Engineering";
        AddTab(_combatInfo, 
            _mode.MoveAttackMouseMode,
            () => _mode.SelectedUnit.Value is not null,
            DrawCombatInfo);
        AddTab(_engineeringInfo, 
            _mode.EngineerMouseMode,
            () => _mode.SelectedUnit.Value?.Components
                    .Get<EngineerEntityComponent>(_client.Data)
                    is not null,
            DrawEngineeringInfo);
    }
    private void DrawCombatInfo()
    {
        _combatInfo.ClearChildren();
        var vbox = new VBoxContainer();
        _combatInfo.AddChild(vbox);
        var unit = _mode.SelectedUnit.Value;
        var regime = unit.Regime.Get(_client.Data);
        var model = unit.UnitModel.Get(_client.Data);
        var texture = new TextureRect();
        texture.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
        texture.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
        texture.Texture = model.GetTexture();
        texture.CustomMinimumSize = Vector2.One * 150f;
        vbox.AddChild(texture);
        vbox.CreateLabelAsChild(model.Name);
        vbox.CreateLabelAsChild($"Hitpoints: {unit.CurrentHitPoints} / {model.HitPoints}");
        vbox.AddChild(new VSeparator());
        vbox.CreateLabelAsChild($"Hardness: {model.Hardness}");

        var hex = unit.GetHex(_client.Data);
        var supplyAvailability = SupplyLogic.GetSupplyAvailability(hex,
            _client.Data);
        vbox.CreateLabelAsChild($"Supply Availability: {supplyAvailability}");
        
        foreach (var unitComponent in unit.Components.All(_client.Data))
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
                DrawTabs, _client);
            _client.SubmitCommand(com);
        });
        reinforce.Disabled = unit.CanReinforce(_client.Data) == false;
        
        var mobilize = vbox.AddButton("Mobilize", () =>
        {
            MobilizeUnitWindow.Open(unit, _client);
        });
        mobilize.Disabled = MobilizerComponent.CanAddRightNow(unit, _client.Data) == false;
    }

    private void DrawEngineeringInfo()
    {
        _engineeringInfo.ClearChildren();
        var vbox = new VBoxContainer();
        _engineeringInfo.AddChild(vbox);
        var unit = _mode.SelectedUnit.Value;
        var e = unit.Components.Get<EngineerEntityComponent>(_client.Data);

        var hex = unit.GetHex(_client.Data);
        var airbase = _client.Data.ModelPredefs.Buildings.Airbase;
        
        var hasLoc = hex.TryGetLocation(_client.Data, out var loc);
        var bs = _client.Data.Models.GetModels<ConstructableBuildingModel>()
            .Where(b => b.AllowedLandforms.Contains(hex.Landform.Get(_client.Data)));
        var hexBuildingProjects = _client.Data.EngineerProjects
            .BuildingConstructionProgresses;
        
        addBuildingButton(airbase);


        void addBuildingButton(ConstructableBuildingModel building)
        {
            if (building.AllowedLandforms.Contains(hex.Landform.Get(_client.Data)) == false)
            {
                return;
            }

            if (hasLoc && loc.Buildings
                    .Any(b => b.Get(_client.Data)
                              == building))
            {
                return;
            }
            
            string text;
            if (hexBuildingProjects.TryGetValue(hex.MakeRef(), out var progresses)
                && progresses.TryGetValue(building.MakeIdRef(_client.Data), out var progress))
            {
                text = $"Work on {building.Name} {progress} / {building.EngineerPointCost}";
            }
            else
            {
                text = $"Build {building.Name}";
            }
            
            
            vbox.AddButton(text,
                () =>
                {
                    if (hasLoc == false)
                    {
                        var loc = new Location(0,
                            hex.MakeRef(), new List<ModelIdRef<BuildingModel>>());
                        var locCom = new EntityCreationCommand<Location>(loc);
                        _client.SubmitCommand(locCom);
                    }

                    var progress = Mathf.Min(building.EngineerPointCost, 
                        e.CurrentEngineerPoints);
                    var proc = new WorkOnBuildingProcedure(hex.MakeRef(),
                        unit.MakeRef(), progress,
                        building.MakeIdRef<ConstructableBuildingModel>(_client.Data));
                    var inner = new DoProcedureCommand(proc);
                    var callback = CallbackCommand.Construct(inner, () =>
                    {
                        if(IsInstanceValid(this)) DrawEngineeringInfo();
                    }, _client);
                    _client.SubmitCommand(callback);
                });
        }
    }
}