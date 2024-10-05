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
    public UnitAttackAction(
        SettingsOption<Unit> selectedUnit,
        HexGeneralClient client,
        MouseButtonMask button) : base(button)
    {
        _client = client;
        _selectedUnit = selectedUnit;
        _radiusOverlay = new MapOverlayDrawer((int)GraphicsLayers.Debug, _client.GetComponent<MapGraphics>);
        _pathOverlay = new MapOverlayDrawer((int)GraphicsLayers.Debug, _client.GetComponent<MapGraphics>);
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
            || unit.Components.OfType<IUnitCombatComponent>(_client.Data)
                .Any(c => c.AttackBlocked(_client.Data)))
        {
            return;
        }
    }
    protected override void MouseDown(InputEventMouse m)
    {
        
    }

    private bool Valid()
    {
        var unit = _selectedUnit.Value;
        if (unit is null)
        {
            return false;
        }

        var targetHex = MouseOverHandler.FindMouseOverHex(_client);
        if (targetHex is null) return false;
        var pos = _client.GetComponent<CameraController>().GetGlobalMousePosition();
        var unitGraphics = _client.GetComponent<MapGraphics>()
            .Units;
        var targetUnit = unitGraphics.GetClosestUnitInHex(targetHex, 
            pos, _client);
        if (targetUnit is null) return false;
        if (targetUnit.Regime == unit.Regime) return false;

        if (unit.Components.OfType<AttackComponent>(_client.Data)
                .Any(a => a.CanAttack(targetUnit, targetHex, _client.Data))
            == false)
        {
            return false;
        }
        
        if(unit.Components.OfType<IUnitCombatComponent>(_client.Data)
               .FirstOrDefault(c => c.AttackBlocked(_client.Data))
           is IUnitCombatComponent cc)
        {
            // GD.Print("blocked by " + cc.GetType().Name);
            return false;
        }



        return true;
    }
    protected override void MouseHeld(InputEventMouse m)
    {
        _pathOverlay.Clear();

        if (Valid() == false) return;
        
        var unit = _selectedUnit.Value;
        var targetHex = MouseOverHandler.FindMouseOverHex(_client);
        var pos = _client.GetComponent<CameraController>().GetGlobalMousePosition();
        var unitGraphics = _client.GetComponent<MapGraphics>()
            .Units;
        
        var targetUnit = unitGraphics.GetClosestUnitInHex(targetHex, 
            pos, _client);
        var unitHex = unit.GetHex(_client.Data);
        
        _pathOverlay.Draw(mb =>
        {
            mb.AddArrow(unitHex.WorldPos(), targetHex.WorldPos(), .25f, Colors.White);
            mb.AddArrow(unitHex.WorldPos(), targetHex.WorldPos(), .2f, Colors.Red);
        }, Vector2.Zero);
        _pathOverlay.DrawUnitHighlightBox(targetUnit, Colors.Red, _client);
        
        var tt = SceneManager.Instance<AttackTooltip>();
        tt.DrawInfo(targetHex, unit, targetUnit, _client.Data);
        _pathOverlay.AddNode(tt, pos);
    }

    protected override void MouseUp(InputEventMouse m)
    {
        _pathOverlay.Clear();

        if (Valid() == false) return;
        
        var unit = _selectedUnit.Value;
        var targetHex = MouseOverHandler.FindMouseOverHex(_client);
        var pos = _client.GetComponent<CameraController>().GetGlobalMousePosition();
        var unitGraphics = _client.GetComponent<MapGraphics>()
            .Units;
        var targetUnit = unitGraphics.GetClosestUnitInHex(targetHex, 
            pos, _client);
        var inner = new UnitAttackCommand(unit.MakeRef(), 
            targetUnit.MakeRef());
        _radiusOverlay.Clear();
        var com = CallbackCommand.Construct(inner,
            () => DrawForSelectedUnit(unit), _client);
        _client.SubmitCommand(com);
    }

    public override void Clear()
    {
        _radiusOverlay.Clear();
        _pathOverlay.Clear();
    }
}