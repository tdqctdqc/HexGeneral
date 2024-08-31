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
    private static int _chunkSize = 30;
    public static Map Generate(HexGeneralData data, 
        NewGameData setupData)
    {
        var map = Map.Create(data);
        MakeHexes(data, setupData);
        DoLandSea(data, setupData);
        DoLandforms(data, setupData);
        DoVegetations(data, setupData);
        MakeChunks(data, setupData);
        MakeSociety(data, setupData);
        
        return map;
    }

    private static void MakeHexes(HexGeneralData data, NewGameData setupData)
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
    private static void DoLandSea(HexGeneralData data, NewGameData setupData)
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
                
            hex.SetLandform(lf.MakeIdRef(data));
        }
    }

    private static void DoLandforms(HexGeneralData data, NewGameData setupData)
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


        var lfsByRoughness = data.Models.GetModels<Landform>()
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
            hex.SetLandform(lf.MakeIdRef(data));
        }
    }

    private static void DoVegetations(HexGeneralData data, NewGameData setupData)
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
                hex.SetVegetation(barren.MakeIdRef(data));
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
            hex.SetVegetation(v.MakeIdRef(data));
        }
    }

    private static void MakeChunks(HexGeneralData data, NewGameData setupData)
    {
        MakeTwigs(data, setupData);
        MakeTopBranches(data, setupData);
        WriteBranchColors(setupData.TopBranches, data, 0);
    }

    private static void MakeTopBranches(
        HexGeneralData data, NewGameData setupData)
    {
        var landHexes = data.Map.Hexes
            .Values
            .Where(h => h.Landform.Get(data).IsLand).ToHashSet();

        Hex getTwigSeed(Branch<Hex> b)
        {
            return ((IAggregate<Branch<Hex>, Hex>)b).Seed;
        }
        
        Func<Branch<Hex>, Branch<Hex>, bool> canShare 
            = (b, c) => 
                landHexes.Contains(getTwigSeed(b))
                == landHexes.Contains(getTwigSeed(c));

        Func<HashSet<Branch<Hex>>, IEnumerable<Branch<Hex>>> seedFactoryFactory()
        {
            return SeedFuncs.NoNeighborSeeds(canShare, h => h.Neighbors, 1);
        }

        Picker<Branch<Hex>> pickerFactory(IEnumerable<Branch<Hex>> seeds)
        {
            return new Picker<Branch<Hex>>(seeds, b => b.Neighbors);
        }
        
        IPickerAgent<Branch<Hex>> agentFactory(Branch<Hex> seed, Picker<Branch<Hex>> picker)
        {
            return new HeuristicPickerAgent<Branch<Hex>>(seed, picker,
                x => canShare(seed, x), 
                (t, r) => getTwigSeed(t).WorldPos().DistanceTo(getTwigSeed(r).WorldPos()), 
                3);
        }

        var topBranches = TreeAggregator
            .BuildTiers(setupData.Twigs,
                seedFactoryFactory,
                pickerFactory,
                agentFactory,
                canShare,
                Branch<Hex>.ConstructTrunk,
                6, 2
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
            var hexes = branch.GetLeaves();
            foreach (var hex in hexes)
            {
                hex.Colors.Add(color);
            }

            if (branch.Children is not null)
            {
                WriteBranchColors(branch.Children, data, colIter);
            }
        }
    }

    private static void MakeTwigs(HexGeneralData data,
        NewGameData setupData)
    {
        var map = data.Map;
        var hexes = map.Hexes
            .Values
            .ToHashSet();
        var landHexes = hexes.Where(h => h.Landform.Get(data).IsLand).ToHashSet();
        Func<Hex, Hex, bool> canShareBase = (h, g) => landHexes.Contains(h) == landHexes.Contains(g);
        
        var picker = new Picker<Hex>(landHexes, 
            h => h.GetNeighbors(data).Where(landHexes.Contains));
        
        var noNeighbors = SeedFuncs
            .NoNeighborSeeds(canShareBase,
                h => h.GetNeighbors(data)
                    .Where(picker.NotTaken.Contains),
                5);
        
        var toPick = 20;
        IPickerAgent<Hex> agentFactory(Hex seed)
        {
            return new HeuristicPickerAgent<Hex>(seed, 
                picker, 
                landHexes.Contains, 
                (t, r) => t.WorldPos().DistanceTo(r.WorldPos()), 
                toPick);
        }
        var res =  TreeAggregator
            .Aggregate(
                noNeighbors, 
                picker,
                agentFactory,
                Branch<Hex>.ConstructTwig,
                h => h.GetNeighbors(data).Where(landHexes.Contains));
        var consolidated = TreeAggregator
            .Consolidate<Branch<Hex>, Hex>(res, 10, 
                (b,c) => landHexes.Contains(b.Leaves.First()) == landHexes.Contains(c.Leaves.First()));
        res.RemoveAll(consolidated.Contains);
        setupData.Twigs.AddRange(res);
        
        foreach (var twig in res)
        {
            var col = ColorsExt.GetRandomColor();
            foreach (var twigLeaf in twig.Leaves)
            {
                twigLeaf.Colors.Add(col);
            }
        }
    }

    private static void MakeSociety(HexGeneralData data, NewGameData setupData)
    {
        MakeChunkUrbans(data, setupData);
        MakeRoadNetwork(data, setupData);
        GenerateRegimes(data, setupData);
    }
    private static void MakeChunkUrbans(HexGeneralData data, NewGameData setupData)
    {
        var map = data.Map;
        var landHexes = data.Map.Hexes.Values
            .Where(h => h.Landform.Get(data).IsLand)
            .ToHashSet();
        var landTwigs = setupData.Twigs
            .Where(t => landHexes.Contains(t.TwigSeed))
            .ToHashSet();
        
        var urban = data.ModelPredefs.Landforms.Urban;
        var barren = data.ModelPredefs.Vegetations.Barren;
        
        Func<Branch<Hex>, Branch<Hex>, bool> canShare = (t, r) => true;
        var getSeeds = SeedFuncs
            .NoNeighborSeeds(
                canShare,
                t => t.Neighbors,
                3);
        var picker = new Picker<Branch<Hex>>(
            landTwigs, b => b.Neighbors);
        Func<Branch<Hex>, Branch<Hex>, float> heuristic = (b, c) 
            => b.TwigSeed.WorldPos().DistanceTo(c.TwigSeed.WorldPos());
        IPickerAgent<Branch<Hex>> agentFactory(Branch<Hex> seed)
        {
            return new BlobPickerAgent<Branch<Hex>>(seed,
                picker,b => true);
        }
        
        var urbanTrunks = TreeAggregator
            .Aggregate(getSeeds,
                picker, 
                agentFactory,
                Branch<Hex>.ConstructTrunk,
                b => b.Neighbors.Where(landTwigs.Contains));
        
        var extraUrbanHexes = new       
        int[]{
                0, 0, 0, 0, 0, 0, 0, 0, 0,
                1, 1, 1, 1, 1,
                2, 2, 2, 2,
                3, 3, 3,
                4, 4, 4,
                5, 5,
                6 };
        foreach (var urbanTrunk in urbanTrunks)
        {
            var seedHex = urbanTrunk.TrunkSeed.TwigSeed;
            seedHex.SetLandform(urban.MakeIdRef(data));
            seedHex.SetVegetation(barren.MakeIdRef(data));
            var moreUrban = extraUrbanHexes[data.Random.RandiRange(0, extraUrbanHexes.Length - 1)];
            if (moreUrban > 0)
            {
                var index = data.Random.RandiRange(0, 5);
                for (var i = 0; i < moreUrban; i++)
                {
                    var coord = seedHex.Coords 
                                + HexExt.HexDirs[index % 6];
                    index++;
                    if (map.Hexes.TryGetValue(coord, out var nHex)
                        && landHexes.Contains(nHex))
                    {
                        nHex.SetLandform(urban.MakeIdRef(data));
                        nHex.SetVegetation(barren.MakeIdRef(data));
                    }
                }
            }
                
            var leaves = urbanTrunk.GetLeaves();
            var color = ColorsExt.GetRandomColor();
            foreach (var leaf in leaves)
            {
                leaf.Colors.Add(color);
            }
            setupData.UrbanTrunks.Add(urbanTrunk);
        }
    }
    private static void MakeRoadNetwork(HexGeneralData data, NewGameData setupData)
    {
        var roads = new RoadNetwork(data.IdDispenser.TakeId(), new Dictionary<Vector2I, ModelIdRef<RoadModel>>());
        data.Entities.AddEntity(roads, data);

        var landHexes = data.Map.Hexes.Values
            .Where(h => h.Landform.Get(data).IsLand)
            .ToHashSet();
        var landTwigs = setupData.Twigs
            .Where(t => landHexes.Contains(t.TwigSeed)).ToHashSet();

        float getHexEdgeCost(Hex h, Hex n)
        {
            var hLf = h.Landform.Get(data);
            var nLf = n.Landform.Get(data);
            return 1f + Mathf.Clamp(hLf.MinRoughness, 0f, 1f) + Mathf.Clamp(nLf.MinRoughness, 0f, 1f);
        }
        
        float heuristic(Hex v, Hex w) => v.WorldPos().DistanceTo(w.WorldPos());
        
        var dirt = data.ModelPredefs.RoadModels.Dirt.MakeIdRef(data);
        var sw = new Stopwatch();
        sw.Start();
        
        var hierarchicalPathFinder 
                = HierarchicalCachedPathFinder<Branch<Hex>, Hex>
                    .ConstructAStar(
                        (b1, b2) => heuristic(b1.TwigSeed, b2.TwigSeed),
                        h => h.Neighbors.Where(landTwigs.Contains),
                        heuristic,
                        b => b.TwigSeed,
                        getHexEdgeCost,
                        h => h.GetNeighbors(data).Where(landHexes.Contains)
                );
        var urbanTrunksBySeed = setupData.UrbanTrunks
            .ToDictionary(t => t.Id,
                t => t);
        hierarchicalPathFinder.MakeNetworkPaths(
            setupData.UrbanTrunks.Select(b => b.TrunkSeed),
            b => urbanTrunksBySeed[b.Id]
                .Neighbors.Select(n => n.TrunkSeed));
        foreach (var ((x, y), path) in hierarchicalPathFinder.PathCache)
        {
            
            for (var i = 0; i < path.Count - 1; i++)
            {
                var from = path[i];
                var to = path[i + 1];
                var key = from.GetIdEdgeKey(to);
                roads.Roads.TryAdd(key, dirt);
            }
        }
        
        sw.Stop();
        GD.Print($"time {sw.Elapsed.TotalMilliseconds}");
    }

    private static void GenerateRegimes(HexGeneralData data, NewGameData setupData)
    {
        
    }
}