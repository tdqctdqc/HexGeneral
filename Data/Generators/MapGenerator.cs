using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Godot;
using GodotUtilities.CSharpExt;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.AggregateTree;
using GodotUtilities.DataStructures.Geometry;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.DataStructures.Noise;
using GodotUtilities.DataStructures.PathFinder;
using GodotUtilities.DataStructures.Picker;
using GodotUtilities.DataStructures.Tree;
using GodotUtilities.GameData;

namespace HexGeneral.Game.Generators;

public static class MapGenerator
{
    private static int _chunkSize = 20;
    public static int BranchTreeDepth = 5;
    private static int _chunkConsolidateNumToPick = 2;

    public static void Generate(HexGeneralData data, 
        GenerationData setupData)
    {
        var h = (int)setupData.Settings.Height.Value;
        var w = (int)setupData.Settings.Width.Value;
        Map.Create(new Vector2I(w, h), data);
        MakeHexes(data, setupData);
        DoLandSea(data, setupData);
        DoLandforms(data, setupData);
        DoVegetations(data, setupData);
        MakeChunks(data, setupData);
    }

    private static void MakeHexes(HexGeneralData data, GenerationData setupData)
    {
        var map = data.Map;
        var h = (int)setupData.Settings.Height.Value;
        var w = (int)setupData.Settings.Width.Value;
        for (var i = 0; i < h; i++)
        {
            for (var j = 0; j < w; j++)
            {
                var gridCoord = new Vector2I(j, i);
                var coord = gridCoord.GridCoordsToCube();
                var hex = new Hex(coord, default, default,
                    data.IdDispenser.TakeId(), new ERef<Regime>());
                map.Hexes.Add(coord, hex);
                map.CoordsById.Add(hex.Id, coord);
            }
        }
    }
    private static void DoLandSea(HexGeneralData data, GenerationData setupData)
    {
        var map = data.Map;
        var landSeaNoise = new TieredNoise();
        landSeaNoise.AddFastNoiseTier(new(-1f, 1f),
            .005f * setupData.Settings.NoiseScale.Value);
        landSeaNoise.AddFastNoiseTier(new(-.5f, .5f),
            .01f * setupData.Settings.NoiseScale.Value);
        landSeaNoise.AddFastNoiseTier(new(-.25f, .25f),
            .05f * setupData.Settings.NoiseScale.Value);
        
        var landRatio = setupData.Settings.LandRatio.Value;
        var landCutoff = 2f * landRatio - 1f;
        
        foreach (var (coord, hex) in map.Hexes)
        {
            var worldPos = coord.CubeToGridCoords().GetWorldPos();
            var landSeaSample = landSeaNoise.GetSample2D(worldPos);
            Landform lf;
            if (landSeaSample <= landCutoff)
            {
                lf = data.ModelPredefs.Landforms.Plain;
            }
            else
            {
                lf = data.ModelPredefs.Landforms.Sea;
            }
                
            hex.SetLandformGen(lf.MakeIdRef(data));
        }
    }

    private static void DoLandforms(HexGeneralData data, GenerationData setupData)
    {
        var map = data.Map;

        var isRoughNoise = new TieredNoise();
        isRoughNoise.AddFastNoiseTier(new(0f, 1f),
            .01f);
        var isRoughCutoff = setupData.Settings.Roughness.Value;
        
        var roughnessNoise = new TieredNoise();
        roughnessNoise.AddFastNoiseTier(new(-.35f, .35f),
            .005f * setupData.Settings.NoiseScale.Value);
        roughnessNoise.AddFastNoiseTier(new(-.25f, .5f),
            .01f * setupData.Settings.NoiseScale.Value);
        roughnessNoise.AddFastNoiseTier(new(-.25f, 1f),
            .05f * setupData.Settings.NoiseScale.Value);


        var lfsByRoughness = data.Models
            .GetModels<Landform>()
            .Where(lf => lf.CanChooseForGen)
            .OrderByDescending(lf => lf.MinRoughness).ToList();
        foreach (var (coord, hex) in map.Hexes)
        {
            var currLf = hex.Landform.Get(data);
            if (currLf.IsLand == false) continue;
            var worldPos = coord.CubeToGridCoords().GetWorldPos();
            
            var isRoughSample = isRoughNoise.GetSample2D(worldPos);
            if (isRoughSample > isRoughCutoff) continue;
            var roughnessSample = roughnessNoise.GetSample2D(worldPos);
            roughnessSample = Mathf.Clamp(roughnessSample, 0f, 1f);
            Landform lf = lfsByRoughness.First(r => roughnessSample >= r.MinRoughness);
            hex.SetLandformGen(lf.MakeIdRef(data));
        }
    }

    private static void DoVegetations(HexGeneralData data, GenerationData setupData)
    {
        var map = data.Map;
        var moistureLevel = setupData.Settings.Moisture.Value;
        var moistureNoise = new TieredNoise();
        moistureNoise.AddFastNoiseTier(new(-moistureLevel, moistureLevel * 2f),
            .003f * setupData.Settings.NoiseScale.Value);
        moistureNoise.AddFastNoiseTier(
            new(-moistureLevel / 2f, moistureLevel / 2f),
            .01f * setupData.Settings.NoiseScale.Value);

        var deforestNoise = new TieredNoise();
        var deforestCutoff = .6f;
        deforestNoise.AddFastNoiseTier(new Vector2(0f, 1f),
            .1f * setupData.Settings.NoiseScale.Value);

        var vsByMoisture = data.Models.GetModels<Vegetation>()
            .OrderByDescending(v => v.MinMoisture).ToList();
        var barren = data.ModelPredefs.Vegetations.Barren;
        var forest = data.ModelPredefs.Vegetations.Forest;
        var grassland = data.ModelPredefs.Vegetations.Grassland;
        
        foreach (var (coord, hex) in map.Hexes)
        {
            var lf = hex.Landform.Get(data);
            if (lf.IsLand == false)
            {
                hex.SetVegetationGen(barren.MakeIdRef(data));
                continue;
            }
            var worldPos = coord.CubeToGridCoords().GetWorldPos();

            var moistureSample = moistureNoise.GetSample2D(worldPos);
            moistureSample = Mathf.Clamp(moistureSample, 0f, 1f);
            Vegetation v = vsByMoisture
                .First(m => 
                    m.AllowedLandforms.Contains(lf)
                    && moistureSample >= m.MinMoisture);
            if (v == forest)
            {
                var deforestSample = deforestNoise.GetSample2D(worldPos);
                if (deforestSample > deforestCutoff)
                {
                    v = grassland;
                }
            }
            hex.SetVegetationGen(v.MakeIdRef(data));
        }
    }

    private static void MakeChunks(HexGeneralData data, GenerationData setupData)
    {
        MakeTwigs(data, setupData);
        MakeTopBranches(data, setupData);
        WriteBranchColors(setupData.TopBranches, data, 0);
    }

    private static void MakeTopBranches(
        HexGeneralData data, GenerationData setupData)
    {
        var landHexes = data.Map.Hexes
            .Values
            .Where(h => h.Landform.Get(data).IsLand).ToHashSet();
        
        Func<Branch<Hex>, Branch<Hex>, bool> canShare 
            = (b, c) => 
                landHexes.Contains(b.GetTwigSeed())
                == landHexes.Contains(c.GetTwigSeed());

        Func<HashSet<Branch<Hex>>, IEnumerable<Branch<Hex>>> seedFactoryFactory()
        {
            return SeedFuncs.NoNeighborSeeds(canShare, h => h.Neighbors, 1);
        }

        Picker<Branch<Hex>> pickerFactory(IEnumerable<Branch<Hex>> seeds)
        {
            return new Picker<Branch<Hex>>(seeds, 
                b => b.Neighbors);
        }
        
        IPickerAgent<Branch<Hex>> agentFactory(Branch<Hex> seed, Picker<Branch<Hex>> picker)
        {
            return new AdjacencyCountPickerAgent<Branch<Hex>>(seed, picker,
                _chunkConsolidateNumToPick,
                x => canShare(seed, x));
        }

        var topBranches = TreeAggregator
            .BuildTiers(setupData.Twigs,
                seedFactoryFactory,
                pickerFactory,
                agentFactory,
                canShare,
                Branch<Hex>.ConstructTrunk,
                BranchTreeDepth, 2
            );
        
        setupData.TopBranches.AddRange(topBranches);
    }

    private static void WriteBranchColors(
        IEnumerable<Branch<Hex>> branches,
        HexGeneralData data,
        int colIter)
    {
        foreach (var branch in branches)
        {
            var baseColor
                = branch.GetFirstLeaf().Landform.Get(data).IsLand
                    ? Colors.Green
                    : Colors.Blue;
            
            var color = baseColor.GetPeriodicShade(colIter++);
            if(branch.Neighbors.Count() == 0) color = Colors.Black;
            var hexes = branch.GetLeaves();
            foreach (var hex in hexes)
            {
                hex.DebugColors.Add(color);
            }

            if (branch.Children is not null)
            {
                WriteBranchColors(branch.Children, data, colIter);
            }
        }
    }

    private static void MakeTwigs(HexGeneralData data,
        GenerationData setupData)
    {
        var map = data.Map;
        var hexes = map.Hexes
            .Values
            .ToHashSet();
        var landHexes = hexes
            .Where(h => h.Landform.Get(data).IsLand)
            .ToHashSet();
        bool canShareBase(Hex h, Hex g)
        {
            return landHexes.Contains(h) == landHexes.Contains(g);
        }

        var picker = new Picker<Hex>(hexes, 
            h => h.GetNeighbors(data));
        
        var noNeighbors = SeedFuncs
            .NoNeighborSeeds<Hex>(canShareBase,
                h => h.GetNeighbors(data),
                5);

        var firstRemaining = SeedFuncs.GetFirstRemainingSeed<Hex>();
        
        var toPick = 10;
        IPickerAgent<Hex> agentFactory(Hex seed)
        {
            var thisPick = landHexes.Contains(seed) ? toPick : 200;
            var land = landHexes.Contains(seed);
            return new AdjacencyCountPickerAgent<Hex>(seed, 
                picker, 
                thisPick, 
                h => landHexes.Contains(h) == land 
            );
        }

        var res =  TreeAggregator
            .Aggregate(
                noNeighbors, 
                picker,
                agentFactory,
                Branch<Hex>.ConstructTwig,
                h => h.GetNeighbors(data));

        var consolidated = TreeAggregator
            .Consolidate<Branch<Hex>, Hex>(res, 10, 
                (b,c) => canShareBase(b.GetTwigSeed(), c.GetTwigSeed()));
        res.RemoveAll(consolidated.Contains);
        setupData.Twigs.AddRange(res);
    }
}