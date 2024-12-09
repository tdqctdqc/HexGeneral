using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.Graphics;

namespace HexGeneral.Game.Client.Graphics;

public partial class RoadGraphics : Node2D
{
    private HexGeneralData _data;
    private float _roadWidth = .2f;
    private Dictionary<RoadModel, List<(Vector3I i, Vector3I f)>> _spokes;
    private Dictionary<RoadModel, List<(Vector3I i, Vector3I f)>[]> _hubs;
    private Dictionary<RoadModel, List<(Vector3I i, Vector3I f)>> _ends;

    public RoadGraphics(HexGeneralData data)
    {
        ZIndex = (int)GraphicsLayers.Roads;
        ZAsRelative = false; 
        _data = data;

        _spokes = new Dictionary<RoadModel, List<(Vector3I, Vector3I)>>();
        _ends = new Dictionary<RoadModel, List<(Vector3I i, Vector3I f)>>();
        _hubs = new Dictionary<RoadModel, List<(Vector3I i, Vector3I f)>[]>();
        Setup();
        Update();
        DrawRoads();
    }

    public void Update()
    {
        var map = _data.Map;
        var roads = _data.RoadNetwork;
        var roadHexIds = roads.Roads
            .Keys.Select(k => k.X).Concat(roads.Roads
                .Keys.Select(k => k.Y))
            .ToHashSet();
        
        foreach (var roadHexId in roadHexIds)
        {
            var coord = map.CoordsById[roadHexId];
            var fromCoord = Vector3I.MaxValue;
            RoadModel fromModel = null;
            bool hasHub = false;
            for (var i = 0; i < 6; i++)
            {
                var nCoord = coord + HexExt.HexDirs[i];
                if (map.Hexes.TryGetValue(nCoord, out var n) == false
                    || roads.Roads.TryGetValue(roadHexId.GetIdEdgeKey(n.Id), 
                        out var rm) == false)
                {
                    continue;
                }

                fromCoord = nCoord;
                var roadModel = rm.Get(_data);
                fromModel = roadModel;
                addSpoke(coord, nCoord, roadModel);
                for (var j = i + 1; j < 6; j++)
                {
                    var n2Coord = coord + HexExt.HexDirs[j];
                    if (map.Hexes.TryGetValue(n2Coord, out var n2) == false
                        || roads.Roads.TryGetValue(roadHexId.GetIdEdgeKey(n2.Id), 
                            out var rm2) == false)
                    {
                        continue;
                    }

                    hasHub = true;
                    var interval = j - i - 1;
                    addHub(coord, nCoord, interval, roadModel);
                }
            }

            if (fromCoord == Vector3I.MaxValue) throw new Exception();

            if (hasHub == false)
            {
                addEnd(coord, fromCoord, fromModel);
            }
        }

        void addSpoke(Vector3I inHex, Vector3I fromHex, RoadModel rm)
        {
            _spokes[rm].Add((inHex, fromHex));
        }

        void addEnd(Vector3I inHex, Vector3I fromHex, RoadModel rm)
        {
            _ends[rm].Add((inHex, fromHex));
        }
        
        void addHub(Vector3I inHex, Vector3I fromHex, int interval,
            RoadModel rm)
        {
            _hubs[rm][interval].Add((inHex, fromHex));
        }
    }

    private void DrawRoads()
    {
        this.ClearChildren();
        var hexSize = new Vector2(2f, HexExt.HexHeight * 2f);

        foreach (var (rm, value) in _spokes)
        {
            var textureName = rm.Name + "Spoke";
            var spoke = TextureManager.GetTexture(textureName);
            var spokeMm = getMmi(hexSize, spoke);
            spokeMm.Multimesh.InstanceCount = value.Count;
            for (var i = 0; i < value.Count; i++)
            {
                var inHex = value[i].i;
                var fromHex = value[i].f;
                var angle = (inHex.GetWorldPos() - fromHex.GetWorldPos())
                    .Rotated(Mathf.Pi / 2f)
                    .Angle();
                spokeMm.Multimesh.SetInstanceTransform2D(i, new Transform2D(angle, inHex.GetWorldPos()));
            }
        }
        
        foreach (var (rm, value) in _ends)
        {
            var end = TextureManager.GetTexture(rm.Name + "End");
            var endMm = getMmi(hexSize / 5f, end);
            endMm.Multimesh.InstanceCount = value.Count;
            for (var i = 0; i < value.Count; i++)
            {
                var inHex = value[i].i;
                var fromHex = value[i].f;
                var angle = (inHex.GetWorldPos() - fromHex.GetWorldPos())
                    .Rotated(Mathf.Pi / 2f)
                    .Angle();
                endMm.Multimesh.SetInstanceTransform2D(i,
                    new Transform2D(angle, inHex.GetWorldPos()));
            }
        }

        var tops = new List<MultiMeshInstance2D>();
        var bottoms = new List<MultiMeshInstance2D>();
        foreach (var (rm, value) in _hubs)
        {
            for (var i = 0; i < value.Length; i++)
            {
                var hubs = value[i];
                var hubTop = TextureManager.GetTexture(rm.Name + "Hub"+ i + "Top");
                var hubTopMm = getMmi(hexSize / 5f, hubTop);
                hubTopMm.Multimesh.InstanceCount = hubs.Count;
                tops.Add(hubTopMm);
                var hubBottom = TextureManager.GetTexture(rm.Name + "Hub"+ i + "Bottom");
                var hubBottomMm = getMmi(hexSize / 5f, hubBottom);
                hubBottomMm.Multimesh.InstanceCount = hubs.Count;
                bottoms.Add(hubBottomMm);
                for (var j = 0; j < value[i].Count; j++)
                {
                    var inHex = value[i][j].i;
                    var fromHex = value[i][j].f;
                    var angle = (inHex.GetWorldPos() - fromHex.GetWorldPos())
                        .Rotated(Mathf.Pi / 2f)
                        .Angle();
                    hubTopMm.Multimesh.SetInstanceTransform2D(j, 
                        new Transform2D(angle, inHex.GetWorldPos()));
                    hubBottomMm.Multimesh.SetInstanceTransform2D(j, 
                        new Transform2D(angle, inHex.GetWorldPos()));
                }
            }
        }
        foreach (var mmi in bottoms)
        {
            RemoveChild(mmi);
            AddChild(mmi);
        }
        foreach (var mmi in tops)
        {
            RemoveChild(mmi);
            AddChild(mmi);
        }
        
        MultiMeshInstance2D getMmi(Vector2 size, Texture2D texture)
        {
            var mmi = new MultiMeshInstance2D();
            var mm = new MultiMesh();
            mmi.Multimesh = mm;
            var m = new QuadMesh();
            m.Size = size;
            mm.Mesh = m;
            mmi.Texture = texture;
            AddChild(mmi);
            return mmi;
        }
    }
    private void Setup()
    {
        foreach (var rm in _data.Models.GetModels<RoadModel>())
        {
            _ends.Add(rm, new List<(Vector3I i, Vector3I f)>());
            _spokes.Add(rm, new List<(Vector3I i, Vector3I f)>());
            _hubs.Add(rm, new List<(Vector3I i, Vector3I f)>[5]);
            for (var i = 0; i < 5; i++)
            {
                _hubs[rm][i] = new List<(Vector3I i, Vector3I f)>();
            }
        }
    }
}