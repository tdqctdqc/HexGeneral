using System.Collections.Generic;
using Godot;
using GodotUtilities.DataStructures.Hex;

namespace HexGeneral.Game.Client.Graphics;

public partial class TerrainDecals : Node2D
{
    private Dictionary<ITerrainAspect, List<MultiMeshInstance2D>> _decals;
    private Dictionary<ITerrainAspect, List<List<Vector2>>> _decalPs;
    private Dictionary<ITerrainAspect, List<Vector2>> _ps;
    private Dictionary<ITerrainAspect, List<Vector2>> _offsets;
    private readonly HexGeneralData _data;

    public TerrainDecals(HexGeneralData data)
    {
        _data = data;
        ZIndex = (int)GraphicsLayers.Terrain;
        ZAsRelative = false;
        _ps = new Dictionary<ITerrainAspect, List<Vector2>>();
        _decals = new Dictionary<ITerrainAspect, List<MultiMeshInstance2D>>();
        _decalPs = new Dictionary<ITerrainAspect, List<List<Vector2>>>();
        _offsets = new Dictionary<ITerrainAspect, List<Vector2>>();
        foreach (var veg in data.Models.GetModels<Vegetation>())
        {
            AddAspect(veg);
        }
        foreach (var lf in data.Models.GetModels<Landform>())
        {
            AddAspect(lf);
        }
        
        foreach (var hex in data.Map.Hexes.Values)
        {
            DoHex(hex);
        }
        foreach (var (aspect, ps) in _ps)
        {
            int iter = 0;
            var decalCount = _decals[aspect].Count;
            if (decalCount == 0) continue;
            
            foreach (var p in ps)
            {
                _decalPs[aspect][iter++ % decalCount].Add(p);
            }
            
            for (var i = 0; i < _decals[aspect].Count; i++)
            {
                var mmi = _decals[aspect][i];
                var poses = _decalPs[aspect][i];
                var offset = _offsets[aspect][i];
                mmi.Multimesh.InstanceCount = poses.Count;
                for (var j = 0; j < poses.Count; j++)
                {
                    mmi.Multimesh.SetInstanceTransform2D(j, 
                        new Transform2D(Mathf.Pi, poses[j] + offset));
                }
            }
        }
    }

    private void AddAspect(ITerrainAspect aspect)
    {
        _ps.Add(aspect, new List<Vector2>());
        _decals.Add(aspect, new List<MultiMeshInstance2D>());
        _decalPs.Add(aspect, new List<List<Vector2>>());
        _offsets.Add(aspect, new List<Vector2>());
        int iter = 1;
        while (true)
        {
            var decalName = aspect.Name + iter.ToString();
            if (TextureManager.Textures.TryGetValue(decalName.ToLower(),
                    out var texture))
            {
                iter++;
                var mmi = new MultiMeshInstance2D();
                AddChild(mmi);
                mmi.Texture = texture;
                var quad = new QuadMesh();
                quad.Size = texture.GetSize();
                quad.Size /= quad.Size.X;
                quad.Size *= aspect.DecalWidth;
                _offsets[aspect]
                    .Add(Vector2.Zero);
                    // .Add(new Vector2(0f, -quad.Size.Y / 2f));
                var multimesh = new MultiMesh();
                multimesh.Mesh = quad;
                mmi.Multimesh = multimesh;
                mmi.YSortEnabled = true;
                _decals[aspect].Add(mmi);
                _decalPs[aspect].Add(new List<Vector2>());
            }
            else
            {
                break;
            }
        }
    }
    private void DoHex(Hex hex)
    {
        var veg = hex.Vegetation.Get(_data);
        if (_decals[veg].Count > 0)
        {
            var ps = UniformPoissonDiskSampler.SampleCircle(
                hex.WorldPos(), .95f, 
                veg.DecalDist);
            _ps[veg].AddRange(ps);
        }

        var lf = hex.Landform.Get(_data);
        if (_decals[lf].Count > 0)
        {
            var ps = UniformPoissonDiskSampler.SampleCircle(
                hex.WorldPos(), .95f, 
                lf.DecalDist);
            _ps[lf].AddRange(ps);
        }
    }
}