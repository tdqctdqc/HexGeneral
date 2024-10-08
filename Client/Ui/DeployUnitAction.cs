using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Command;
using HexGeneral.Game.Components;

namespace HexGeneral.Client.Ui;

public class DeployUnitAction(MouseButtonMask button,
    DeploymentMode _mode,
    HexGeneralClient _client)
    : MouseAction(button)
{
    protected override void MouseDown(InputEventMouse m)
    {
    }

    protected override void MouseHeld(InputEventMouse m)
    {
    }

    protected override void MouseUp(InputEventMouse m)
    {
        var unit = _mode.SelectedUnit.Value;
        if (unit is null) return;
        var unitDomain = unit.Components.Get<IMoveComponent>(_client.Data)
            .GetActiveMoveType(_client.Data).Domain;
        var mousePos = _client.GetComponent<CameraController>()
            .GetGlobalMousePosition();
        var regime = _client.GetPlayer()?.Regime.Get(_client.Data);
        if (regime is null) return;
        var hex = MouseOverHandler.FindMouseOverHex(_client);
        if (hex.Regime != regime || hex.CanDeploy(unitDomain, _client.Data) == false)
        {
            return;
        }

        var com = new DeployUnitCommand(unit.MakeRef(), hex.MakeRef());
        _client.SubmitCommand(com);
    }

    public override void Clear()
    {
        
    }
}