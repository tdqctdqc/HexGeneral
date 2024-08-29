using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.CSharpExt;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.AggregateTree;
using GodotUtilities.DataStructures.Geometry;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.DataStructures.Noise;
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
        MakeChunkUrbans(data, setupData);
        MakeLandmasses(data, setupData);
        MakeRoadNetwork(data, setupData);
        
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
                var hex = new Hex(coord, default, default);
                map.Hexes.Add(coord, hex);
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
            // GD.Print(roughnessSample);
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
        var map = data.Map;
        var baseBranches = makeBase();
        Func<Branch<Hex>, Branch<Hex>, bool> canShare = (b, c) =>
        {
            return b.GetFirstLeaf().Landform.Get(data).IsLand
                == c.GetFirstLeaf().Landform.Get(data).IsLand;
        };
        var topBranches = TreeAggregator
            .BuildTiers(baseBranches,
                () => SeedFuncs
                    .NoNeighborSeeds(canShare,
                        h => h.Neighbors,
                        1),
                () => PickerFuncs.ChooseRandom(
                    3,
                    h => h.Neighbors,
                    canShare, data.Random),
                canShare,
                () => new Branch<Hex>(),
                4, 2
            );
            
            
            
            // aggregate(baseBranches, 
            // 4, 3, 2);
        
        foreach (var branch in topBranches)
        {
            var leaves = branch.GetLeaves();
            var color = ColorsExt.GetRandomColor();
            foreach (var leaf in leaves)
            {
                leaf.Chunk1Color = color;
            }
        }
        
        
        
        List<Branch<Hex>> makeBase()
        {
            var hexes = map.Hexes
                .Values
                .ToHashSet();
            var landHexes = hexes.Where(h => h.Landform.Get(data).IsLand).ToHashSet();
            Func<Hex, Hex, bool> canShare = (h, g) => landHexes.Contains(h) == landHexes.Contains(g);
            var noNeighbors = SeedFuncs
                .NoNeighborSeeds(canShare,
                    h => h.GetNeighbors(data),
                    5);
            var firstRemaining = SeedFuncs.GetFirstRemainingSeed<Hex>();

            var choose = PickerFuncs.ChooseRandom<Hex>(_chunkSize, h => h.GetNeighbors(data),
                canShare, data.Random);
            
            var res =  TreeAggregator
                .Aggregate(
                    noNeighbors, 
                    choose, 
                    hexes,
                    () => new Branch<Hex>(),
                    h => h.GetNeighbors(data));
            var consolidated = TreeAggregator
                .Consolidate<Branch<Hex>, Hex>(res, 10, 
                    (b,c) => landHexes.Contains(b.Leaves.First()) == landHexes.Contains(c.Leaves.First()));
            res.RemoveAll(consolidated.Contains);
            return res;
        }

        List<Branch<Hex>> aggregate(List<Branch<Hex>> source, 
            int times, int take, int min)
        {
            var curr = source;
            var landBranches = source.Where(s => s.Leaves.First().Landform.Get(data).IsLand)
                .ToHashSet();
            
            for (var i = 0; i < times; i++)
            {
                Func<Branch<Hex>, Branch<Hex>, bool> canShare = (h, g) => landBranches.Contains(h) == landBranches.Contains(g);
            
                var noNeighbors 
                    = SeedFuncs
                        .NoNeighborSeeds(canShare,
                            h => h.Neighbors,
                            1);
                var choose 
                    = PickerFuncs.ChooseRandom<Branch<Hex>>(take,
                        h => h.Neighbors,
                        canShare, data.Random);
                
                var next = TreeAggregator.Aggregate(
                    noNeighbors,
                    choose,
                    curr.ToHashSet(),
                    () => new Branch<Hex>(),
                    b => b.Neighbors
                );
                foreach (var branch in next)
                {
                    if (landBranches.Contains(branch.Children.First()))
                    {
                        landBranches.Add(branch);
                    }
                }

                curr = next;
                var c = TreeAggregator
                    .Consolidate<Branch<Hex>, Branch<Hex>>(
                        curr, min, canShare);
                curr.RemoveAll(c.Contains);
            }
            
            
            return curr;
        }
    }

    private static void MakeChunkUrbans(HexGeneralData data, NewGameData setupData)
    {
        // var urban = data.ModelPredefs.Landforms.Urban;
        // var barren = data.ModelPredefs.Vegetations.Barren;
        // var chunks = setupData.HexBranches
        //     .SelectMany(b => b.GetLeaves())
        //     .EnumerableToHashSet();
        //
        // var map = data.Map;
        // var exclude = new HashSet<Branch<Hex>>();
        //
        // foreach (var chunk in chunks)
        // {
        //     if (exclude.Contains(chunk)) continue;
        //     if (chunk.Children.Count < 4) continue; 
        //     if (chunk.Children.First().Landform.Get(data).IsLand == false) continue;
        //     if (data.Random.Randf() < .75f) continue;
        //     var numUrbanHexes = data.Random.RandiRange(1, 5);
        //     numUrbanHexes = Mathf.Min(chunk.Children.Count / 3, numUrbanHexes);
        //     var flood = FloodFill<Hex>
        //         .FloodFillRandomishToLimit(chunk.Seed, numUrbanHexes,
        //             h => h.GetNeighbors(data),
        //             h => chunk.Children.Contains(h), 
        //             data.Random);
        //     foreach (var hex in flood)
        //     {
        //         hex.SetLandform(urban.MakeIdRef(data));
        //         hex.SetVegetation(barren.MakeIdRef(data));
        //     }
        //     foreach (var nChunk in chunk.Neighbors(data))
        //     {
        //         exclude.Add(nChunk);
        //     }
        // }
    }
    private static void MakeLandmasses(HexGeneralData data, NewGameData setupData)
    {
        // var map = data.Map;
        // var chunks = setupData.HexBranches.SelectMany(b => b.GetLeaves())
        //     .EnumerableToHashSet();
        // var landHexes = map.Hexes.Values
        //     .Where(h => h.Landform.Get(data).IsLand)
        //     .EnumerableToHashSet();
        // var seaHexes = map.Hexes.Values.Except(landHexes)
        //     .EnumerableToHashSet();
        //
        // var landUnions = UnionFind.Find<Hex, HashSet<Hex>>(
        //         landHexes, (t, r) => true, t => t.GetNeighbors(data))
        //     .Select(hs => hs.Select(h => h.Coords).EnumerableToHashSet())
        //     .ToList();
        //
        // var seaUnions = UnionFind.Find<Hex, HashSet<Hex>>(
        //         seaHexes, (t, r) => true, t => t.GetNeighbors(data))
        //     .Select(hs => hs.Select(h => h.Coords).EnumerableToHashSet())
        //     .ToList();
        // var allUnions = landUnions.Concat(seaUnions).ToList();
        // var dic = allUnions.ToDictionary(v => v, v => new HashSet<int>());
        //
        // foreach (var chunk in chunks)
        // {
        //     var union = allUnions.First(u => u.Contains(chunk.Seed));
        //     dic[union].Add(id);
        // }
        // var landmasses = new LandSeaMasses(data.IdDispenser.TakeId(),
        //     dic.Values.ToList());
        // data.Entities.AddEntity(landmasses, data);
    }
    private static void MakeRoadNetwork(HexGeneralData data, NewGameData setupData)
    {
        // var map = data.Map;
        // var urban = data.ModelPredefs.Landforms.Urban;
        // var chunks = data.HexChunks;
        // var landmasses = data.LandSeaMasses;
        //
        // foreach (var massChunkIds in landmasses.MassChunkIds)
        // {
        //     var firstChunk = chunks.Chunks[massChunkIds.First()];
        //     if (firstChunk.IsLand == false) continue;
        //     var massChunks = massChunkIds
        //         .Select(id => chunks.Chunks[id]).ToArray();
        //     var urbanChunks = massChunks
        //         .Where(c => map.Hexes[c.Seed].Landform.Get(data) == urban);
        //         
        //
        //     var greatChunkPicker = new Picker<HexChunk>(massChunks,
        //         h => h.Neighbors(data));
        //     foreach (var urbanSeed in urbanChunks)
        //     {
        //         greatChunkPicker.AddAgent(
        //             new AdjacencyCountPickerAgent<HexChunk>(urbanSeed, greatChunkPicker, 1, h => true));
        //     }
        //     greatChunkPicker.RandomAgentPick();
        //     
        //     foreach (var agent in greatChunkPicker.Agents)
        //     {
        //         
        //     }
        // }
        //
        //
        //
        // List<Hex> getPath(Hex start, Hex end)
        // {
        //     if (start.WorldPos().DistanceTo(end.WorldPos()) > 20)
        //     {
        //         return new();
        //     }
        //     return PathFinder<Hex>.FindPath(
        //         start, end, v => v.GetNeighbors(data)
        //             .Where(n => n.Landform.Get(data).IsLand),
        //         (v, w) =>
        //         {
        //             var vLf = v.Landform.Get(data);
        //             var wLf = w.Landform.Get(data);
        //             return 1f + vLf.MinRoughness + wLf.MinRoughness;
        //         },
        //         (v, w) => v.WorldPos().DistanceTo(w.WorldPos()));
        // }
        
    }
}