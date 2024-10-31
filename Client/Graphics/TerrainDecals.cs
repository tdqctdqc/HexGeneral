using System.Collections.Generic;
using Godot;
using GodotUtilities.DataStructures.Hex;

namespace HexGeneral.Game.Client.Graphics;

public partial class TerrainDecals : Node2D
{
    private Dictionary<Vegetation, List<MultiMeshInstance2D>> _vegDecals;
    private Dictionary<Vegetation, List<List<Vector2>>> _vegDecalPs;
    private Dictionary<Vegetation, List<Vector2>> _vegPs;
    private float _decalWidth = .5f;
    private readonly HexGeneralData _data;

    public TerrainDecals(HexGeneralData data)
    {
        _data = data;
        ZIndex = (int)GraphicsLayers.Terrain;
        ZAsRelative = false;
        _vegPs = new Dictionary<Vegetation, List<Vector2>>();
        _vegDecals = new Dictionary<Vegetation, List<MultiMeshInstance2D>>();
        _vegDecalPs = new Dictionary<Vegetation, List<List<Vector2>>>();
        foreach (var veg in data.Models.GetModels<Vegetation>())
        {
            _vegPs.Add(veg, new List<Vector2>());
            _vegDecals.Add(veg, new List<MultiMeshInstance2D>());
            _vegDecalPs.Add(veg, new List<List<Vector2>>());
            int iter = 1;
            while (true)
            {
                var decalName = veg.Name + iter.ToString();
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
                    quad.Size *= _decalWidth;
                    var multimesh = new MultiMesh();
                    multimesh.Mesh = quad;
                    mmi.Multimesh = multimesh;
                    _vegDecals[veg].Add(mmi);
                    _vegDecalPs[veg].Add(new List<Vector2>());
                }
                else
                {
                    break;
                }
            }
        }
        
        foreach (var hex in data.Map.Hexes.Values)
        {
            DoHex(hex);
        }
        foreach (var (veg, ps) in _vegPs)
        {
            
            int iter = 0;
            var decalCount = _vegDecals[veg].Count;
            if (decalCount == 0) continue;
            
            foreach (var p in ps)
            {
                _vegDecalPs[veg][iter++ % decalCount].Add(p);
            }
            
            for (var i = 0; i < _vegDecals[veg].Count; i++)
            {
                var mmi = _vegDecals[veg][i];
                var poses = _vegDecalPs[veg][i];

                mmi.Multimesh.InstanceCount = poses.Count;
                for (var j = 0; j < poses.Count; j++)
                {
                    mmi.Multimesh.SetInstanceTransform2D(j, 
                        new Transform2D(Mathf.Pi, poses[j]));
                }
            }
        }
    }

    private void DoHex(Hex hex)
    {
        var veg = hex.Vegetation.Get(_data);
        if (_vegDecals[veg].Count > 0)
        {
            var ps = UniformPoissonDiskSampler.SampleCircle(
                hex.WorldPos(), HexExt.HexHeight * .9f, 
                _decalWidth);
            _vegPs[veg].AddRange(ps);
        }
    }
}