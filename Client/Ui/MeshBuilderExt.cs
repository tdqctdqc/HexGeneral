using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Game.Components;

namespace HexGeneral.Client.Ui;

public static class MeshBuilderExt
{
    public static void DrawUnitHighlightBox(this MapOverlayDrawer overlay,
        Unit unit,
        Color color,
        HexGeneralClient client)
    {
        var domain = unit.Components.Get<IMoveComponent>(client.Data).GetActiveMoveType(client.Data)
            .Domain;
        var hexUnits 
            = client.Data.MapUnitHolder.HexUnitsByDomain
                    [domain.MakeIdRef(client.Data)]
                [unit.GetHex(client.Data).MakeRef()];
        var index = hexUnits.IndexOf(unit.MakeRef());
        var offset = UnitGraphics.GraphicOffsetsByNumUnits[hexUnits.Count - 1][index];

        overlay.Draw(mb =>
        {
            mb.DrawBox((Vector2.Left + Vector2.Up) / 2f,
                (Vector2.Right + Vector2.Down) / 2f,
                color, .1f);
        }, unit.GetHex(client.Data).WorldPos() + offset);
    }
}