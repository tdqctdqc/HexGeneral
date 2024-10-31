using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Godot;
using GodotUtilities.CSharpExt;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.Graph;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.Graphics;
using Poly2Tri;

namespace HexGeneral.Game.Client.Graphics;

public partial class TerrainFancyMesh : Node2D
{
    private HexGeneralData _data;
    private static Vector3 _lightDir = _lightDir 
        = new Vector3(0f, -1f, -.5f);

    public TerrainFancyMesh(HexGeneralData data)
    {
        _data = data;
    }

    public override void _Ready()
    {
        ZIndex = (int)GraphicsLayers.Terrain;
        ZAsRelative = false;
        var mb = new MeshBuilder();
        var unions = UnionFind.Find<Hex, HashSet<Hex>>(
            _data.Map.Hexes.Values,
            (h, i) 
                => h.Landform.Equals(i.Landform)
                        && h.Vegetation.Equals(i.Vegetation),
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
        var lf = union.First().Landform.Get(_data);

        // if (lf == _data.ModelPredefs.Landforms.Sea) return;
        var color = union.First().GetTerrainColor(_data);
        var outlines = GetUnionOutlineAndHoles(union);

        List<Vector2> steiners;
        
        
        if (lf == _data.ModelPredefs.Landforms.Hill)
        {
            steiners = GetSteinerPoints(union, .25f);
        }
        else if (lf == _data.ModelPredefs.Landforms.Mountain)
        {
            steiners = GetSteinerPoints(union, .5f);
        }
        else
        {
            steiners = union.Select(h =>
                    h.WorldPos() + (Vector2.Up * .5f)
                    .Rotated(_data.Random.RandfRange(0f, Mathf.Pi * 2f)))
                .ToList();
        }
        

        int iter = 0;


        var poly = GetPoly(outlines, steiners);


        if (lf == _data.ModelPredefs.Landforms.Mountain)
        {
            BuildMountainMesh(poly, mb);
        }
        else
        {
            var wobble = .05f;
            if (lf == _data.ModelPredefs.Landforms.Hill)
            {
                wobble = .25f;
            }
            else if (lf == _data.ModelPredefs.Landforms.Sea)
            {
                wobble = .1f;
            }
            var altitudes = GetPointAltitudes(poly);
            BuildDefaultMesh(poly, altitudes, color, wobble, mb);
        }
    }

    private Polygon GetPoly(List<List<(Vector3I, int)>> outlines,
        List<Vector2> interiors)
    {
        var outer = outlines.MaxBy(Span);
        var poly = new Polygon(outer.Select(getPolyPoint));
        
        for (var i = 0; i < outlines.Count; i++)
        {
            var outline = outlines[i];
            if (outline == outer) continue;
            poly.AddHole(new Polygon(outline.Select(getPolyPoint)));
        }
        poly.AddSteinerPoints(interiors
            .Select(p => new TriangulationPoint(p.X, p.Y)).ToList());
        
        P2T.Triangulate(poly);
        return poly;
        
        PolygonPoint getPolyPoint((Vector3I, int) v)
        {
            var p = GetP(v.Item1, v.Item2);
            return new PolygonPoint(p.X, p.Y);
        }
    }

    private Dictionary<Vector2, float> GetPointAltitudes(Polygon poly)
    {
        var res = new Dictionary<Vector2, float>();

        foreach (var t in poly.Points)
        {
            var p = GetV2(t);
            res.Add(p, 0f);
        }

        if (poly.Holes is not null)
        {
            foreach (var hole in poly.Holes)
            {
                foreach (var t in hole.Points)
                {
                    var p = GetV2(t);
                    res.Add(p, 0f);
                }
            }
        }
        
        
        
        foreach (var t in poly.Triangles)
        {
            var a = GetV2(t.Points[0]);
            if (res.ContainsKey(a) == false)
            {
                var alt = _data.Random.RandfRange(0f, 1f);
                res.Add(a, alt);
            }
            
            var b = GetV2(t.Points[1]);
            if (res.ContainsKey(b) == false)
            {
                var alt = _data.Random.RandfRange(0f, 1f);
                res.Add(b, alt);
            }
            
            var c = GetV2(t.Points[2]);
            if (res.ContainsKey(c) == false)
            {
                var alt = _data.Random.RandfRange(0f, 1f);
                res.Add(c, alt);
            }
        }
        
        return res;
    }

    private void BuildMountainMesh(Polygon poly, MeshBuilder mb)
    {
        var (ps, borderPs, edges) 
            = SetupPsAndEdges(poly);

        var peaks = GetPeaks(ps, borderPs, edges);
        var altitudes = new Dictionary<int, float>();
        foreach (var i in ps.Values1)
        {
            if (peaks.Contains(i))
            {
                altitudes.Add(i, _data.Random.RandfRange(.75f, 1f));
            }
            else if(borderPs.Contains(i))
            {
                altitudes.Add(i, 0);
            }
            else
            {
                altitudes.Add(i, _data.Random.RandfRange(.25f, .5f));
            }
        }
        
        
        foreach (var t in poly.Triangles)
        {
            var a = GetV2(t.Points[0]);
            var aI = ps[a];
            var b = GetV2(t.Points[1]);
            var bI = ps[b];

            var c = GetV2(t.Points[2]);
            var cI = ps[c];
            var score = 0;
            if (peaks.Contains(aI)) score++;
            if (peaks.Contains(bI)) score++;
            if (peaks.Contains(cI)) score++;
            
            
            var peak = score > 1;

            var color = peak ? Colors.White : new Color("4f4443");

            var a3 = new Vector3(a.X, a.Y, altitudes[aI]);
            var b3 = new Vector3(b.X, b.Y, altitudes[bI]);
            var c3 = new Vector3(c.X, c.Y, altitudes[cI]);

            color = color.Darkened(GetDarkenRatio(a3, b3, c3, .5f));
            mb.AddTri(a, b, c, color);
        }
        
    }

    private HashSet<int> GetPeaks(Bijection<int, Vector2> ps, HashSet<int> borderPs, AdjacencyGraph<int> edges)
    {
        var availForPeaks = ps.Values1.Except(borderPs).ToHashSet();
        var peakSingles = new HashSet<int>();
        var currRange = new List<int>();
        while (availForPeaks.Count > 0)
        {
            currRange.Clear();
            var num = _data.Random.RandiRange(1, 10);
            var first = availForPeaks.First();
            availForPeaks.Remove(first);
            peakSingles.Add(first);
            currRange.Add(first);
            if (num == 1 
                || edges[first].Any(availForPeaks.Contains) == false)
            {
                foreach (var n in edges[first])
                {
                    availForPeaks.Remove(n);
                }
            }
            else
            {
                var curr = first;
                for (var i = 0; i < num - 1; i++)
                {
                    var next = edges[curr]
                        .FirstOrDefault(n => availForPeaks.Contains(n));
                    if (next == 0) break;
                    peakSingles.Add(next);
                    availForPeaks.Remove(next);
                    var overlap = edges[curr].Intersect(edges[next]);
                    foreach (var j in overlap)
                    {
                        availForPeaks.Remove(j);
                    }
                    currRange.Add(next);
                    curr = next;
                }
                
                foreach (var i in currRange)
                {
                    foreach (var j in edges[i])
                    {
                        availForPeaks.Remove(j);
                        foreach (var k in edges[j])
                        {
                            availForPeaks.Remove(k);
                        }
                    }
                }
            }
        }

        return peakSingles;
    }


    private (Bijection<int, Vector2> ps,
        HashSet<int> borderPs,
        AdjacencyGraph<int> edges) SetupPsAndEdges(Polygon poly)
    {
        var ps = new Bijection<int, Vector2>();
        var borderPs = new HashSet<int>();
        var edges = new AdjacencyGraph<int>(new Dictionary<int, HashSet<int>>());
        int iter = 1;
        foreach (var t in poly.Points)
        {
            var p = GetV2(t);
            var i = iter++;
            ps.Add(i, p);
            borderPs.Add(i);
        }

        if (poly.Holes is not null)
        {
            foreach (var hole in poly.Holes)
            {
                foreach (var t in hole.Points)
                {
                    var p = GetV2(t);
                    var i = iter++;
                    ps.Add(i, p);
                    borderPs.Add(i);
                }
            }
        }
        
        
        
        foreach (var t in poly.Triangles)
        {
            var a = GetV2(t.Points[0]);
            var b = GetV2(t.Points[1]);
            var c = GetV2(t.Points[2]);
            
            addP(a);
            addP(b);
            addP(c);
            addEdge(a, b);
            addEdge(a, c);
            addEdge(b, c);

            void addP(Vector2 p)
            {
                if (ps.Contains(p) == false)
                {
                    var i = iter++;
                    ps.Add(i, p);
                }
            }

            void addEdge(Vector2 p1, Vector2 p2)
            {
                var i1 = ps[p1];
                var i2 = ps[p2];
                edges.AddEdge(i1, i2);
            }
        }

        return (ps, borderPs, edges);
    }
    private void BuildDefaultMesh(Polygon poly, 
        Dictionary<Vector2, float> altitudes,
        Color color, float wobble, MeshBuilder mb)
    {
        foreach (var t in poly.Triangles)
        {
            var a2 = GetV2(t.Points[0]);
            var a = new Vector3(
                a2.X,
                a2.Y,
                altitudes[a2]
            );
            
            var b2 = GetV2(t.Points[1]);
            var b = new Vector3(
                b2.X,
                b2.Y,
                altitudes[b2]
            );
            
            var c2 = GetV2(t.Points[2]);
            var c = new Vector3(
                c2.X,
                c2.Y,
                altitudes[c2]
            );

            var ratio = GetDarkenRatio(a, b, c, wobble);
            
            mb.AddTri(a2, b2, c2, 
                color.Darkened(ratio));
        }
    }

    private float GetDarkenRatio(Vector3 a, Vector3 b, Vector3 c,
        float wobble)
    {
        var normal = (a - c).Cross(b - c);
        var angle = _lightDir.AngleTo(normal);
            
        return angle
            .ProjectToRange(new Vector2(0f, Mathf.Pi),
                new Vector2(-wobble, wobble));
    }
    private float Span(List<(Vector3I, int)> outline)
    {
        var maxX = float.MinValue;
        var minX = float.MaxValue;
        for (var i = 0; i < outline.Count; i++)
        {
            var p = GetP(outline[i].Item1, outline[i].Item2);
            maxX = Mathf.Max(maxX, p.X);
            minX = Mathf.Min(minX, p.X);
        }

        return Mathf.Abs(maxX - minX);
    }

    private Vector2 GetP(Vector3I coords, int dir)
    {
        var p1 = coords.GetWorldPos();
        var p2 = (coords + HexExt.HexDirs[dir]).GetWorldPos();
        var mid = (p1 + p2) / 2f;
        var perp = (p1 - p2).Orthogonal().Normalized();
        return mid + perp * .5f;
    }

    private Vector2 GetV2(TriangulationPoint t)
    {
        return new Vector2((float)t.X, (float)t.Y);
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

    private List<Vector2> GetSteinerPoints(HashSet<Hex> union, 
        float dist)
    {
        var res = new List<Vector2>();
        foreach (var hex in union)
        {
            var steiners = UniformPoissonDiskSampler.SampleCircle(hex.WorldPos(),
                HexExt.HexHeight * .9f, dist);
            res.AddRange(steiners);
        }

        return res;
    }

    
}