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
    public static int BranchTreeDepth = 5;
    public static int UrbanGenDepth => BranchTreeDepth - 2;
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
                3,
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
        
        var urban = data.ModelPredefs.Landforms.Urban;
        var barren = data.ModelPredefs.Vegetations.Barren;
        
        var atDepth = setupData
            .TopBranches.GetAtDepth(UrbanGenDepth);
        
        var urbanTrunks = atDepth
                .Where(t => landHexes.Contains(t.GetTwigSeed()))
                .ToHashSet();
            
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
            var seedHex = urbanTrunk.TrunkSeed.GetTwigSeed();
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
        var pathFindDepth = 1;
        var pathFindDepthBranches = setupData.UrbanTrunks
            .GetAtDepth(pathFindDepth)
            .Where(b => landHexes.Contains(b.GetTwigSeed()))
            .ToHashSet();

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
                        (b1, b2) => heuristic(b1.GetTwigSeed(), b2.GetTwigSeed()),
                        h => h.Neighbors.Where(pathFindDepthBranches.Contains),
                        heuristic,
                        b => b.GetTwigSeed(),
                        getHexEdgeCost,
                        h => h.GetNeighbors(data).Where(landHexes.Contains)
                );
        var urbanTrunksById = setupData.UrbanTrunks
            .ToDictionary(t => t.Id,
                t => t);
        var urbanTrunkSeeds = setupData.UrbanTrunks.Select(b => b.GetTrunkSeedAtDepth(pathFindDepth));
        hierarchicalPathFinder.MakeNetworkPaths(
            urbanTrunkSeeds,
            b => urbanTrunksById[b.Id]
                .Neighbors.Select(n => n.GetTrunkSeedAtDepth(pathFindDepth))
                .Where(pathFindDepthBranches.Contains));
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
        var landHexes = data.Map.Hexes.Values
            .Where(h => h.Landform.Get(data).IsLand).ToHashSet();

        var landBranches = setupData.TopBranches
            .Where(b => landHexes.Contains(b.GetTwigSeed()))
            .ToHashSet();
        
        var landmasses = 
            UnionFind.Find<Branch<Hex>, HashSet<Branch<Hex>>>(
                    landBranches,
                    (t, r) => true, 
                    h => h.Neighbors)
            .ToArray();

        var bigLandmasses = landmasses
            .Where(lm => lm.Sum(b => b.GetLeaves().Count()) > 150)
            .ToHashSet();
        var smallLandmasses = landmasses
            .Except(bigLandmasses)
            .ToHashSet();
        var bigHexes = bigLandmasses
            .SelectMany(h => h)
            .SelectMany(h => h.GetLeaves())
            .ToHashSet();
        var smallHexes = smallLandmasses
            .SelectMany(h => h)
            .SelectMany(h => h.GetLeaves())
            .ToHashSet();
        
        var regimeTrunks = landBranches
            .SelectMany(b => b.GetBranchesAtDepth(2))
            .Where(b => bigHexes.Contains(b.GetTwigSeed()))
            .ToHashSet();
        
        var regimeModels = data.Models
            .GetModels<RegimeModel>().ToArray();
        
        for (var i = 0; i < regimeModels.Length; i++)
        {
            GD.Print(i + " " + regimeModels[i].Name);
        }
        
        var picker = new Picker<Branch<Hex>>(regimeTrunks, 
            b => b.Neighbors);
        var seedFactory = SeedFuncs.NoNeighborSeeds<Branch<Hex>>(
            (t, r) => true,
            b => b.Neighbors,
            1);
        
        IPickerAgent<Branch<Hex>> agentFactory(Branch<Hex> seed)
        {
            return new AdjacencyCountPickerAgent<Branch<Hex>>(seed, 
                picker, 1, 
                x => regimeTrunks.Contains(x));
        }
        var regimeIter = 0;
        var regimeTerritories = TreeAggregator
            .Aggregate(seedFactory,
                picker, agentFactory, Branch<Hex>.ConstructTrunk,
                b => b.Neighbors
                    .Where(regimeTrunks.Contains));
        // var consolidated = TreeAggregator
        //     .Consolidate<Branch<Hex>, Branch<Hex>>(regimeTerritories,
        //         10, (b, c) => true);
        // regimeTerritories.RemoveAll(consolidated.Contains);
        
        
        foreach (var regimeTerritory in regimeTerritories)
        {
            var hexes = regimeTerritory.GetLeaves();
            var regimeModel = regimeModels.Modulo(regimeIter++);
            var regime = new Regime(data.IdDispenser.TakeId(),
                regimeModel.MakeIdRef(data));
            data.Entities.AddEntity(regime, data);
            foreach (var hex in hexes)
            {
                if (hex.Regime.Fulfilled()) throw new Exception();
                hex.SetRegime(regime.MakeRef());
            }
        }

        foreach (var smallLandmass in smallLandmasses)
        {
            var first = smallLandmass.First()
                .GetTrunkSeedAtDepth(BranchTreeDepth);
                
            var closeBranch = FloodFill<Branch<Hex>>
                .FindFirst(first,
                    b => true,
                    b => b.Neighbors,
                    b => b.GetTwigSeed().Regime.Fulfilled()
                        && bigHexes.Contains(b.GetTwigSeed()));
            var closeHex = closeBranch.GetTwigSeed();
            var closeRegime = closeHex.Regime.Get(data);
            foreach (var branch in smallLandmass)
            {
                foreach (var hex in branch.GetLeaves())
                {
                    hex.SetRegime(closeRegime.MakeRef());
                }
            }
        }
    }
}