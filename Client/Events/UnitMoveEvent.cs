using System.Collections.Generic;
using System.Threading;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Logic.Procedure;

namespace HexGeneral.Game.Client;

public class UnitMoveEvent : ClientEvent
{
    private Unit _unit;
    private List<HexRef> _path;

    public UnitMoveEvent(Unit unit, List<HexRef> path)
    {
        _unit = unit;
        _path = path;
    }


    public override void Handle(HexGeneralClient client)
    {
        var mapGraphics = client.GetComponent<MapGraphics>();
        var unitGraphics = mapGraphics.Units;
        var graphic = unitGraphics.Graphics[_unit.MakeRef()];
        for (var i = 0; i < _path.Count - 1; i++)
        {
            // var pos = _procedure.Path[i + 1].GetWorldPos();
            // graphic.Position = pos;
            // var hex = client.Data.Map.Hexes[_procedure.Path[i + 1].Coords];
            // mapGraphics.RegimeGraphics.UpdateHex(hex, client);
        }
        new HexRedrawEvent(_path[0]).Handle(client);
        new HexRedrawEvent(_path[^1]).Handle(client);
        new UnitRedrawEvent(_unit).Handle(client);
    }
}