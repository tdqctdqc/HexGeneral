using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.Graphics;

namespace HexGeneral.Game.Client.Graphics;

public partial class RoadGraphics : Node2D
{
    private HexGeneralData _data;

    public RoadGraphics(HexGeneralData data)
    {
        ZIndex = (int)GraphicsLayers.Roads;
        ZAsRelative = false; 
        _data = data;
        _data = data;
        DrawRoads();
    }

    public void DrawRoads()
    {
        this.ClearChildren();
        var map = _data.Map;
        var roads = _data.RoadNetwork;
        var mb = new MeshBuilder();
        foreach (var ((h1, h2), r) in roads.Roads)
        {
            var roadModel = r.Get(_data);
            var p1 = map.Hexes[map.CoordsById[h1]].WorldPos();
            var p2 = map.Hexes[map.CoordsById[h2]].WorldPos();
            mb.AddLine(p1, p2, roadModel.Color, .25f);
        }
        AddChild(mb.GetMeshInstance());
    }
}