using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Game.Components;

namespace HexGeneral.Client.Ui;

public class UnitAttackAction : MouseAction
{
    private SettingsOption<Unit> _selectedUnit;
    private MapOverlayDrawer _radiusOverlay, _pathOverlay;
    private HexGeneralClient _client;
    public UnitAttackAction(MapOverlayDrawer radiusOverlay,
        MapOverlayDrawer pathOverlay,
        SettingsOption<Unit> selectedUnit,
        HexGeneralClient client,
        MouseButtonMask button) : base(button)
    {
        _selectedUnit = selectedUnit;
        _radiusOverlay = radiusOverlay;
        _pathOverlay = pathOverlay;
        _client = client;
        _selectedUnit.SettingChanged.Subscribe(v =>
        {
            DrawForSelectedUnit(v.newVal);
        });
    }

    private void DrawForSelectedUnit(Unit unit)
    {
        _radiusOverlay.Clear();
        _pathOverlay.Clear();
        if (unit is null 
            || unit.Components.Get<AttackCountComponent>()
                .CanAttack(unit, _client.Data) == false)
        {
            return;
        }
        var attackType = unit.UnitModel.Get(_client.Data).AttackType;
        attackType.DrawRadius(unit, _radiusOverlay, _client);
    }
    protected override void MouseDown(InputEventMouse m)
    {
        
    }

    protected override void MouseHeld(InputEventMouse m)
    {
        _pathOverlay.Clear();

        var u = _selectedUnit.Value;
        if (u is null)
        {
            return;
        }

        var hex = MouseOverHandler.FindMouseOverHex(_client);
        if (hex is null) return;
        
        var attackType = u.UnitModel.Get(_client.Data).AttackType;

        if (attackType.CanAttack(u, hex, _client.Data) == false)
        {
            return;
        }
        
        var pos = _client.GetComponent<CameraController>().GetGlobalMousePosition();

        var targetUnit = UnitGraphics.GetClosestUnitInHex(hex, pos, _client);
        if (targetUnit is null) return;
        
        attackType.DrawPath(u, targetUnit, _pathOverlay, _client);
    }

    protected override void MouseUp(InputEventMouse m)
    {
        _pathOverlay.Clear();
        var unit = _selectedUnit.Value;

        if (unit is null)
        {
            return;
        }

        var attackCount = unit.Components.Get<AttackCountComponent>();

        if (attackCount.CanAttack(unit, _client.Data) == false)
        {
            return;
        }
        
        var mouseHex = MouseOverHandler.FindMouseOverHex(_client);
        if (mouseHex is null)
        {
            return;
        }
        var pos = _client.GetComponent<CameraController>().GetGlobalMousePosition();

        var targetUnit = UnitGraphics.GetClosestUnitInHex(mouseHex, pos, _client);
        if (targetUnit is null) return;
        
        
        var inner = unit.UnitModel.Get(_client.Data).AttackType
            .GetAttackCommand(unit, targetUnit, _client);
        if (inner is not null)
        {
            _radiusOverlay.Clear();
            var com = CallbackCommand.Construct(inner,
                () => DrawForSelectedUnit(unit), _client);
            _client.SubmitCommand(com);
        }
    }
}