using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.AggregateTree;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.DataStructures.Picker;
using GodotUtilities.DataStructures.Tree;
using GodotUtilities.GameData;

namespace HexGeneral.Game.Generators;

public static class RegimeGenerator
{
    public static int UrbanGenDepth => MapGenerator.BranchTreeDepth - 2;

    public static void Generate(HexGeneralData data, GenerationData setupData)
    {
        MakeChunkUrbans(data, setupData);
        MakeRoadNetwork(data, setupData);
        GenerateRegimes(data, setupData);
        GenerateUnits(data, setupData);
        var regimes = data.Entities.GetAll<Regime>();

        data.Entities.AddEntity(new TurnManager(data.IdDispenser.TakeId(),
            1,
            regimes.Select(r => r.MakeRef()).ToList(),
            0),
            data);
    }

    

    private static void MakeChunkUrbans(HexGeneralData data, GenerationData setupData)
    {
        var map = data.Map;
        var landHexes = data.Map.Hexes.Values
            .Where(h => h.Landform.Get(data).IsLand)
            .ToHashSet();

        var locationHolder = new LocationHolder(data.IdDispenser.TakeId(),
            new Dictionary<HexRef, ERef<Location>>());
        data.Entities.AddEntity(locationHolder, data);

        var popBuildings = data.Models.GetModels<PopBuilding>()
            .OrderBy(p => p.Pop).ToArray();
        var industryBuildings = data.Models.GetModels<IndustryBuilding>()
            .OrderBy(p => p.IndustrialProd).ToArray();
        
        var urban = data.ModelPredefs.Landforms.Urban;
        var barren = data.ModelPredefs.Vegetations.Barren;
        var airbase = data.ModelPredefs.Buildings.Airbase;
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
        var colIter = 0;
        var port = data.ModelPredefs.Buildings.Port;
        foreach (var urbanTrunk in urbanTrunks)
        {
            var needPort = true;
            var seedHex = urbanTrunk.TrunkSeed.GetTwigSeed();
            seedHex.SetLandform(urban.MakeIdRef(data));
            seedHex.SetVegetation(barren.MakeIdRef(data));
            var loc = new Location(data.IdDispenser.TakeId(),
                seedHex.MakeRef(), new List<ModelIdRef<BuildingModel>>());
            data.Entities.AddEntity(loc, data);
            
            var moreUrban 
                = extraUrbanHexes[data.Random.RandiRange(0, 
                    extraUrbanHexes.Length - 1)];
            if (moreUrban == 0)
            {
                var popBuilding = popBuildings[data.Random.RandiRange(0, 1)];
                loc.Buildings.Add(popBuilding.MakeIdRef<BuildingModel>(data));
                loc.Buildings.Add(industryBuildings[0].MakeIdRef<BuildingModel>(data));
                if (seedHex.GetNeighbors(data)
                        .FirstOrDefault(n => n.Landform.Get(data).IsLand == false)
                            is Hex seaN)
                {
                    needPort = false;
                    addPort(seaN);
                }
            }
            else
            {
                var popLevelMax = moreUrban;
                popLevelMax = Mathf.Min(moreUrban, popBuildings.Length - 1);
                var popLevelMin = moreUrban / 2;
                
                loc.Buildings.Add(popBuildings[popLevelMax].MakeIdRef<BuildingModel>(data));

                var industryMax = Mathf.CeilToInt((float)moreUrban / industryBuildings.Length) ;
                industryMax = Mathf.Clamp(industryMax, 0, industryBuildings.Length - 1);
                
                loc.Buildings.Add(industryBuildings[industryMax].MakeIdRef<BuildingModel>(data));
                loc.Buildings.Add(airbase.MakeIdRef<BuildingModel>(data));
                
                var index = data.Random.RandiRange(0, 5);
                for (var i = 0; i < moreUrban; i++)
                {
                    var coord = seedHex.Coords 
                                + HexExt.HexDirs[index % 6];
                    index++;

                    if (map.Hexes.TryGetValue(coord, out var nHex)
                        && landHexes.Contains(nHex)
                        && locationHolder.Locations.ContainsKey(new HexRef(coord)) == false)
                    {
                        var extraLoc = new Location(data.IdDispenser.TakeId(),
                            new HexRef(coord), 
                            new List<ModelIdRef<BuildingModel>>());
                        data.Entities.AddEntity(extraLoc, data);

                        var extraPopSample = data.Random.RandiRange(popLevelMin, popLevelMax);
                        var extraPopBuilding = popBuildings[extraPopSample];
                        extraLoc.Buildings.Add(extraPopBuilding.MakeIdRef<BuildingModel>(data));

                        var industrySample = Mathf.CeilToInt((float)extraPopSample / industryBuildings.Length) ;
                        industrySample = Mathf.Clamp(industrySample, 0, industryBuildings.Length - 1);
                        
                        var industryBuilding = industryBuildings[industrySample];
                        extraLoc.Buildings.Add(industryBuilding.MakeIdRef<BuildingModel>(data));
                        
                        nHex.SetLandform(urban.MakeIdRef(data));
                        nHex.SetVegetation(barren.MakeIdRef(data));
                        
                        if (needPort && nHex.GetNeighbors(data)
                                .FirstOrDefault(n => n.Landform.Get(data).IsLand == false)
                            is Hex seaN)
                        {
                            needPort = false;
                            addPort(seaN);
                        }
                    }
                }
            }
                
            var leaves = urbanTrunk.GetLeaves();
            var color = Colors.Orange.GetPeriodicShade(colIter++);
            foreach (var leaf in leaves)
            {
                if (leaf == seedHex)
                {
                    leaf.DebugColors.Add(Colors.Purple);
                }
                else
                {
                    leaf.DebugColors.Add(color);
                }
            }
            setupData.UrbanTrunks.Add(urbanTrunk);
        }

        void addPort(Hex seaHex)
        {
            var location = new Location(data.IdDispenser.TakeId(),
                seaHex.MakeRef(), new List<ModelIdRef<BuildingModel>>());
            data.Entities.AddEntity(location, data);
            location.Buildings.Add(port.MakeIdRef<BuildingModel>(data));
        }
    }
    private static void MakeRoadNetwork(HexGeneralData data, GenerationData setupData)
    {
        var roads = new RoadNetwork(data.IdDispenser.TakeId(), new Dictionary<Vector2I, ModelIdRef<RoadModel>>());
        data.Entities.AddEntity(roads, data);
        
        var landHexes = data.Map.Hexes.Values
            .Where(h => h.Landform.Get(data).IsLand)
            .ToHashSet();
        var pathFindDepth = 1;

        float getHexEdgeCost(Hex h, Hex n)
        {
            var hLf = h.Landform.Get(data);
            var nLf = n.Landform.Get(data);
            return 1f + Mathf.Clamp(hLf.MinRoughness, 0f, 1f) + Mathf.Clamp(nLf.MinRoughness, 0f, 1f);
        }
        
        float heuristic(Hex v, Hex w) => v.WorldPos().DistanceTo(w.WorldPos());

        var roadPathFinder = new BranchPathFinder<Hex>(
            heuristic,
            landHexes.Contains,
            getHexEdgeCost,
            h => h.GetNeighbors(data),
            pathFindDepth
        );
        roadPathFinder.FindNetworkPaths(setupData.UrbanTrunks);
        var dirt = data.ModelPredefs.RoadModels.Dirt.MakeIdRef(data);

        foreach (var ((x, y), path) in roadPathFinder.CachedPaths)
        {
            for (var i = 0; i < path.Count - 1; i++)
            {
                var from = path[i];
                var to = path[i + 1];
                var key = from.GetIdEdgeKey(to);
                roads.Roads.TryAdd(key, dirt);
            }
        }
    }

    private static void GenerateRegimes(HexGeneralData data, GenerationData setupData)
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
                regimeModel.MakeIdRef(data), 500f, 500f,
                new HashSet<Vector3I>());
            data.Entities.AddEntity(regime, data);
            foreach (var hex in hexes)
            {
                if (hex.Regime.Fulfilled()) throw new Exception();
                hex.SetRegimeGen(regime.MakeRef());
                regime.Hexes.Add(hex.Coords);
            }
        }

        foreach (var smallLandmass in smallLandmasses)
        {
            var first = smallLandmass.First()
                .GetTrunkSeedAtDepth(MapGenerator.BranchTreeDepth);

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
                    hex.SetRegimeGen(closeRegime.MakeRef());
                    closeRegime.Hexes.Add(hex.Coords);
                }
            }
        }
    }
    
    private static void GenerateUnits(HexGeneralData data, GenerationData setupData)
    {
        var unitHolder = MapUnitHolder.Construct(data);
        
        data.Entities.AddEntity(unitHolder, data);
        var urban = data.ModelPredefs.Landforms.Urban;
        var infantry = data.ModelPredefs.UnitModelPredefs.Infantry;
        var engineer = data.ModelPredefs.UnitModelPredefs.Engineer;
        foreach (var hex in data.Map.Hexes.Values)
        {
            if (hex.Landform.Get(data) == urban)
            {
                var regime = hex.Regime.Get(data);
                var rand = data.Random.RandiRange(1, 3);
                for (var i = 0; i < rand; i++)
                {
                    var unit = i == 0
                        ? engineer.Instantiate(regime, data)
                        : infantry.Instantiate(regime, data);
                    data.Entities.AddEntity(unit, data);
                    unitHolder.DeployUnit(unit, hex, data);
                }
            }
        }
    }
}