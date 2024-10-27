using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.Graphics;
using Poly2Tri.Triangulation;
using Poly2Tri.Triangulation.Polygon;

namespace HexGeneral.Game.Client.Graphics;

public partial class TerrainFancyMesh : Node2D
{
    private HexGeneralData _data;
    public TerrainFancyMesh(HexGeneralData data)
    {
        _data = data;
    }

    public override void _Ready()
    {
        ZIndex = 99;
        ZAsRelative = false;
        var mb = new MeshBuilder();
        var unions = UnionFind.Find<Hex, HashSet<Hex>>(
            _data.Map.Hexes.Values,
            (h, i) => h.Landform.Equals(i.Landform),
            h => h.GetNeighbors(_data)
        );
        int iter = 0;
        foreach (var union in unions)
        {
            DoUnion(union, mb);
        }
        AddChild(mb.GetMeshInstance());
    }

    private void DoUnion(HashSet<Hex> union, MeshBuilder mb)
    {
        if (union.First().Landform.Get(_data) == _data.ModelPredefs.Landforms.Sea) return;
        var outlines = GetUnionOutlineAndHoles(union);
        var outer = outlines.MaxBy(span);
        
        
        
        for (var i = 0; i < outlines.Count; i++)
        {
            var outline = outlines[i];
            var color = outline == outer ? Colors.Green : Colors.Red;
            
            for (var j = 0; j < outline.Count; j++)
            {
                var hex = outline[j].Item1;
                var dir = outline[j].Item2;

                var nHex = hex + HexExt.HexDirs[dir];
                // mb.AddLine(hex.GetWorldPos(), nHex.GetWorldPos(),
                //     color, .25f);
                mb.DrawHexBorder(hex, nHex, color);
            }
        }

        Vector2 getP(Vector3I coords, int dir)
        {
            var p1 = coords.GetWorldPos();
            var p2 = (coords + HexExt.HexDirs[dir]).GetWorldPos();
            var mid = (p1 + p2) / 2f;
            var perp = (p1 - p2).Orthogonal().Normalized();
            return mid + perp * .5f;
        }
        float span(List<(Vector3I, int)> outline)
        {
            var maxX = float.MinValue;
            var minX = float.MaxValue;
            for (var i = 0; i < outline.Count; i++)
            {
                var p = getP(outline[i].Item1, outline[i].Item2);
                maxX = Mathf.Max(maxX, p.X);
                minX = Mathf.Min(minX, p.X);
            }

            return Mathf.Abs(maxX - minX);
        }

        // var interiorPs = GetInteriorPoints(outlines, .5f);
        // mb.AddPointMarkers(interiorPs.ToList(), .05f, 
        //     ColorsExt.GetRandomColor());
        
        
    }

    private List<List<(Vector3I, int)>> GetUnionOutlineAndHoles(HashSet<Hex> union)
    {
        var range6 = Enumerable.Range(0, 6).ToArray();

        if (union.Count == 1)
        {
            var h = union.First();
            return range6.Select(i => (h.Coords, i))
                .ToList().Yield().ToList();
        }

        var res = new List<List<(Vector3I, int)>>();
        
        var faces = union.SelectMany(h =>
        {
            return range6.Where(dir => dirPointsOutside(h.Coords, dir))
                .Select(dir => (h.Coords, dir));
        }).ToHashSet();
        
        
        var facesSource = faces.ToHashSet();
        
        while (faces.Count > 0)
        {
            var subRes = new List<(Vector3I, int)>();
            res.Add(subRes);
            
            var curr = faces.First();
            var first = (curr.Coords, curr.dir);

            while (faces.Count > 0)
            {
                faces.Remove(curr);
                if(facesSource.Contains(curr)) subRes.Add(curr);
                (Vector3I, int) next = (curr.Coords, (curr.dir + 1) % 6);
                
                if (dirPointsOutside(curr.Coords, curr.dir) == false)
                {
                    var nextCoords 
                        = curr.Coords + HexExt.HexDirs[curr.dir];
                    next = (nextCoords, (curr.dir + 4) % 6);
                }
                if (next == first)
                {
                    break;
                }
                curr = next;
            }


            
            
        }


        return res;

        
        
        

        bool dirPointsOutside(Vector3I hex, int dirIndex)
        {
            var neighborCoords = hex + HexExt.HexDirs[dirIndex];
            return _data.Map.Hexes.TryGetValue(neighborCoords, out var n) == false
                   || union.Contains(n) == false;
        }
        

    }

    private Vector2[] GetInteriorPoints(Vector2[] outline, float dist)
    {
        var minX = outline.Min(p => p.X);
        var maxX = outline.Max(p => p.X);
        var minY = outline.Min(p => p.Y);
        var maxY = outline.Max(p => p.Y);

        return UniformPoissonDiskSampler.SampleRectangle(
            new Vector2(minX, minY), new Vector2(maxX, maxY), 
            dist)
            .Where(p => Geometry2D.IsPointInPolygon(p, outline))
            .ToArray();
    }

    private void GetTriangulation(Vector2[] outline, Vector2[] interiorPs)
    {
        
        var poly = new Poly2Tri.Triangulation.Polygon.Polygon(
            outline.Select(p => new PolygonPoint(p.X, p.Y)));
        foreach (var interiorP in interiorPs)
        {
            poly.Add(new TriangulationPoint(interiorP.X, interiorP.Y));
        }
        Poly2Tri.P2T.Triangulate(poly);
        
    }
}