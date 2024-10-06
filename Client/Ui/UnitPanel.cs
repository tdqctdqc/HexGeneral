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

public partial class UnitPanel : TabContainer
{
    private UnitMode _mode;
    private HexGeneralClient _client;
    private VBoxContainer _combatInfo, _engineeringInfo;
    private List<Control> _tabs;
    private List<MouseMode> _tabModes;
    private List<Func<bool>> _tabEnable;
    private List<Action> _tabDraw;
    
    public UnitPanel(UnitMode mode, HexGeneralClient client)
    {
        _client = client;
        _mode = mode;
        _mode.SelectedUnit.SettingChanged.SubscribeForNode(v =>
        {
            DrawTabs();
        }, this);
        _mode.MouseMode.SettingChanged.SubscribeForNode(v =>
        {
            DrawTabs();
        }, this);
        
        TreeEntered += DrawTabs;
        VisibilityChanged += () =>
        {
            if (Visible) DrawTabs();
        };
        _client.Data.Notices.UnitAltered.SubscribeForNode(u =>
        {
            DrawTabs();
        }, this);
        _client.Data.Notices.UnitMoved.SubscribeForNode(u =>
        {
            DrawTabs();
        }, this);
        _client.Data.Notices.FinishedTurnStartLogic.SubscribeForNode(DrawTabs, this);

        _combatInfo = new VBoxContainer();
        _combatInfo.Name = "Combat";
        AddChild(_combatInfo);
        _engineeringInfo = new VBoxContainer();
        _engineeringInfo.Name = "Engineering";
        AddChild(_engineeringInfo);

        _tabs = new List<Control>();
        _tabEnable = new List<Func<bool>>();
        _tabDraw = new List<Action>();
        _tabModes = new List<MouseMode>();
        
        AddTab(_combatInfo, 
            _mode.MoveAttackMouseMode,
            () => true,
            DrawCombatInfo);
        AddTab(_engineeringInfo, 
            _mode.EngineerMouseMode,
            () => _mode.SelectedUnit.Value?.Components.Get<EngineerEntityComponent>(_client.Data)
                    is not null,
            DrawEngineeringInfo);
        TabSelected += v =>
        {
            var index = _tabs.IndexOf((Control)GetChildren()[(int)v]);
            if (_mode.MouseMode.Value != _tabModes[index])
            {
                _mode.MouseMode.Set(_tabModes[index]);
            }
        };
    }

    private void AddTab(Control tab, MouseMode mode, Func<bool> enable, 
        Action draw)
    {
        _tabs.Add(tab);
        _tabModes.Add(mode);
        _tabEnable.Add(enable);
        _tabDraw.Add(draw);
    }
    private void DrawTabs()
    {
        if (_mode.MouseMode.Value == _mode.SelectUnitMouseMode)
        {
            for (var i = 0; i < _tabs.Count; i++)
            {
                SetTabDisabled(i, true);
            }

            return;
        }

        for (var i = 0; i < _tabs.Count; i++)
        {
            var mode = _tabModes[i];
            var index = _tabs[i].GetIndex();
            if (mode == _mode.MouseMode.Value)
            {
                CurrentTab = index;
                _tabDraw[i]();
            }
            SetTabDisabled(index, _tabEnable[i]() == false);
        }
    }

    private void DrawCombatInfo()
    {
        _combatInfo.ClearChildren();
        var unit = _mode.SelectedUnit.Value;
        SetTabDisabled(_combatInfo.GetIndex(), unit is null);

        if (unit is null)
        {
            return;
        }

        var regime = unit.Regime.Get(_client.Data);
        var model = unit.UnitModel.Get(_client.Data);
        var texture = new TextureRect();
        texture.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
        texture.Texture = model.GetTexture();
        texture.Size = Vector2.One * 20f;
        texture.CustomMinimumSize = texture.Size;
        _combatInfo.AddChild(texture);
        _combatInfo.CreateLabelAsChild(model.Name);
        _combatInfo.CreateLabelAsChild($"Hitpoints: {unit.CurrentHitPoints} / {model.HitPoints}");
        _combatInfo.AddChild(new VSeparator());
        _combatInfo.CreateLabelAsChild($"Hardness: {model.Hardness}");

        var hex = unit.GetHex(_client.Data);
        var supplyAvailability = SupplyLogic.GetSupplyAvailability(hex,
            _client.Data);
        _combatInfo.CreateLabelAsChild($"Supply Availability: {supplyAvailability}");
        
        foreach (var unitComponent in unit.Components.All(_client.Data))
        {
            _combatInfo.AddChild(unitComponent.GetDisplay(_client));
        }
        
        
        var reinforce = _combatInfo.AddButton($"Reinforce", () =>
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
        
        var mobilize = _combatInfo.AddButton("Mobilize", () =>
        {
            MobilizeUnitWindow.Open(unit, _client);
        });
        mobilize.Disabled = MobilizerComponent.CanAddRightNow(unit, _client.Data) == false;
    }

    private void DrawEngineeringInfo()
    {
        _engineeringInfo.ClearChildren();
        
        var unit = _mode.SelectedUnit.Value;
        SetTabDisabled(_engineeringInfo.GetIndex(), unit is null);
        if (unit is null)
        {
            return;
        }
        
        var e = unit.Components.Get<EngineerEntityComponent>(_client.Data);
        SetTabDisabled(_engineeringInfo.GetIndex(), e is null);

        if (e is null)
        {
            return;
        }

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
            
            
            _engineeringInfo.AddButton(text,
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