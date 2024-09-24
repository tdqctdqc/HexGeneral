using System.Linq;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Command;
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
            || unit.Components.Components.OfType<IUnitCombatComponent>()
                .Any(c => c.AttackBlocked(_client.Data)))
        {
            return;
        }
    }
    protected override void MouseDown(InputEventMouse m)
    {
        
    }

    protected override void MouseHeld(InputEventMouse m)
    {
        _pathOverlay.Clear();

        var unit = _selectedUnit.Value;
        if (unit is null)
        {
            return;
        }

        var hex = MouseOverHandler.FindMouseOverHex(_client);
        if (hex is null) return;
        
        var attackType = unit.UnitModel.Get(_client.Data).AttackType;
        
        if (attackType.CanAttack(unit, hex, _client.Data) == false)
        {
            return;
        }
        
        if(unit.Components.Components.OfType<IUnitCombatComponent>()
            .Any(c => c.AttackBlocked(_client.Data)))
        {
            return;
        }
        
        var pos = _client.GetComponent<CameraController>().GetGlobalMousePosition();

        var targetUnit = UnitGraphics.GetClosestUnitInHex(hex, pos, _client);
        if (targetUnit is null) return;
        
        var unitHex = unit.GetHex(_client.Data);
        var targetHex = targetUnit.GetHex(_client.Data);

        _pathOverlay.Draw(mb =>
        {
            mb.AddArrow(unitHex.WorldPos(), targetHex.WorldPos(), .25f, Colors.White);
            mb.AddArrow(unitHex.WorldPos(), targetHex.WorldPos(), .2f, Colors.Red);
        }, Vector2.Zero);
        var tt = SceneManager.Instance<AttackTooltip>();
        tt.DrawInfo(targetHex, unit, targetUnit, _client.Data);
        _pathOverlay.AddNode(tt, pos);
    }

    protected override void MouseUp(InputEventMouse m)
    {
        _pathOverlay.Clear();
        var unit = _selectedUnit.Value;
        if (unit is null)
        {
            return;
        }

        if(unit.Components.Components.OfType<IUnitCombatComponent>()
           .Any(c => c.AttackBlocked(_client.Data)))
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
        
        var attackType = unit.UnitModel.Get(_client.Data)
            .AttackType;
        if (attackType.CanAttack(unit, mouseHex, _client.Data) == false)
        {
            return;
        }
        
        var inner = new UnitAttackCommand(unit.MakeRef(), targetUnit.MakeRef());
        _radiusOverlay.Clear();
        var com = CallbackCommand.Construct(inner,
            () => DrawForSelectedUnit(unit), _client);
        _client.SubmitCommand(com);
    }
}