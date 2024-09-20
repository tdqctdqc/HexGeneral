using System.Threading;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Logic.Procedure;

namespace HexGeneral.Game.Client;

public class UnitMoveEvent : ClientEvent
{
    private UnitMoveProcedure _procedure;

    public UnitMoveEvent(UnitMoveProcedure procedure)
    {
        _procedure = procedure;
    }

    public override void Handle(HexGeneralClient client)
    {
        var unit = _procedure.Unit.Get(client.Data);
        var mapGraphics = client.GetComponent<MapGraphics>();
        var unitGraphics = mapGraphics.Units;
        var graphic = unitGraphics.Graphics[unit.MakeRef()];
        for (var i = 0; i < _procedure.Path.Count - 1; i++)
        {
            // var pos = _procedure.Path[i + 1].GetWorldPos();
            // graphic.Position = pos;
            // var hex = client.Data.Map.Hexes[_procedure.Path[i + 1].Coords];
            // mapGraphics.RegimeGraphics.UpdateHex(hex, client);
        }
        new HexRedrawEvent(_procedure.Path[0]).Handle(client);
        new HexRedrawEvent(_procedure.Path[^1]).Handle(client);
        new UnitRedrawEvent(unit).Handle(client);
    }
}