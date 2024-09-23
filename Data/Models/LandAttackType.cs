using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using GodotUtilities.Server;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Command;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Game.Components;

namespace HexGeneral.Game;

public class LandAttackType : AttackType
{
    public override bool CanAttack(Unit unit, Hex target, 
        HexGeneralData data)
    {
        var model = unit.UnitModel.Get(data);
        return unit.GetHex(data).Coords.GetHexDistance(target.Coords) <= model.Range;
    }

    public override void DrawRadius(Unit unit, MapOverlayDrawer overlay, 
        HexGeneralClient client)
    {
        return;
    }

    public override void DrawPath(Unit unit, Unit target, 
        MapOverlayDrawer overlay, HexGeneralClient client)
    {
        var unitHex = unit.GetHex(client.Data);
        var targetHex = target.GetHex(client.Data);
        overlay.Draw(mb =>
        {
            mb.AddArrow(unitHex.WorldPos(), targetHex.WorldPos(), .25f, Colors.White);
            mb.AddArrow(unitHex.WorldPos(), targetHex.WorldPos(), .2f, Colors.Red);
        }, Vector2.Zero);
        var tt = SceneManager.Instance<AttackTooltip>();
        tt.DrawInfo(targetHex, unit, target, client.Data);
        var pos = client.GetComponent<CameraController>().GetGlobalMousePosition();
        overlay.AddNode(tt, pos);
    }

    public override Command GetAttackCommand(Unit unit, Unit target, HexGeneralClient client)
    {
        if (CanAttack(unit, target.GetHex(client.Data), client.Data) == false
            || unit.Components.Get<AttackCountComponent>().CanAttack(unit, client.Data) == false
            || unit.Components.Get<IMoveComponent>().AttackBlocked(client.Data))
        {
            return null;
        }
        return new UnitAttackCommand(unit.MakeRef(), target.MakeRef());
    }
}