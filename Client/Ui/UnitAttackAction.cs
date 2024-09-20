using System.Linq;
using Godot;
using GodotUtilities.CSharpExt;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Client.Ui;
using HexGeneral.Game.Client.Command;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Game.Logic;

namespace HexGeneral.Game.Client;

public class UnitAttackAction : MouseAction
{
    private SettingsOption<Unit> _selectedUnit;
    private MapOverlayDrawer _arrowOverlay, _damageOverlay;
    private HexGeneralClient _client;
    public UnitAttackAction(MouseButtonMask button, 
        SettingsOption<Unit> selectedUnit, 
        MapOverlayDrawer arrowOverlay, MapOverlayDrawer damageOverlay, 
        HexGeneralClient client) : base(button)
    {
        _selectedUnit = selectedUnit;
        _arrowOverlay = arrowOverlay;
        _damageOverlay = damageOverlay;
        _client = client;
    }

    protected override void MouseDown(InputEventMouse m)
    {
    }

    protected override void MouseHeld(InputEventMouse m)
    {
        _arrowOverlay.Clear();
        _damageOverlay.Clear();
        var unit = _selectedUnit.Value;
        if (unit is null || unit.Attacked)
        {
            return;
        }
        
        var regime = unit.Regime.Get(_client.Data);
        
        var pos = _client.GetComponent<CameraController>().GetGlobalMousePosition();
        var mouseHex = MouseOverHandler.FindTwoClosestHexes(pos,
            _client.Data.Map);
        
        if (mouseHex.closest is null)
        {
            return;
        }

        var uHex = _client.Data.MapUnitHolder
            .UnitPositions[unit.MakeRef()];
        var unitHex = _client.Data.Map.Hexes[uHex.Coords];
        
        var hex = mouseHex.closest;
        if (hex.Regime == regime)
        {
            return;
        }
        if (hex.GetNeighbors(_client.Data).Contains(unitHex) == false)
        {
            return;
        }
        
        var targetUnit = UnitGraphics.GetClosestUnitInHex(hex, pos, _client);
        if (targetUnit is null) return;
        
        _arrowOverlay.Draw(mb =>
        {
            mb.AddArrow(unitHex.WorldPos(), hex.WorldPos(), .25f, Colors.White);
            mb.AddArrow(unitHex.WorldPos(), hex.WorldPos(), .2f, Colors.Red);
        }, Vector2.Zero);
        
        
        var damageToTarget = CombatLogic.GetDamageAgainst(unit, targetUnit,
            hex, true, _client.Data);
        var damageMultToTarget = targetUnit.UnitModel.Get(_client.Data)
            .MoveType.GetDamageMult(hex, _client.Data, false);
        
        var damageToUnit = CombatLogic.GetDamageAgainst(targetUnit, unit,
            hex, false, _client.Data);
        var damageMultToUnit = unit.UnitModel.Get(_client.Data)
            .MoveType.GetDamageMult(hex, _client.Data, true);

        var tt = SceneManager.Instance<AttackTooltip>();
        tt.DrawInfo(hex, unit, targetUnit, _client.Data);
        _damageOverlay.AddNode(tt, pos);
    }

    protected override void MouseUp(InputEventMouse m)
    {
        _arrowOverlay.Clear();
        _damageOverlay.Clear();
        var unit = _selectedUnit.Value;
        if (unit is null || unit.Attacked)
        {
            return;
        }
        
        var regime = unit.Regime.Get(_client.Data);

        var pos = _client.GetComponent<CameraController>().GetGlobalMousePosition();
        var mouseHex = MouseOverHandler.FindTwoClosestHexes(pos,
            _client.Data.Map);
        if (mouseHex.closest is null)
        {
            return;
        }

        var unitHex = unit.GetHex(_client.Data);
        var hex = mouseHex.closest;
        if (hex.Regime == regime)
        {
            return;
        }
        
        if (hex.Coords.GetHexDistance(unitHex.Coords) > unit.UnitModel.Get(_client.Data).Range)
        {
            return;
        }
        
       

        var unitsInTargetHex = hex.GetUnitRefs(_client.Data).ToArray();
        if (unitsInTargetHex.Length == 0)
        {
            return;
        }

        var targetUnit = UnitGraphics.GetClosestUnitInHex(hex, pos, _client);
        var com = new UnitAttackCommand(unit.MakeRef(), targetUnit.MakeRef());
        _client.SubmitCommand(com);
    }
}