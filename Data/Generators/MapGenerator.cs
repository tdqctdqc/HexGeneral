using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.CSharpExt;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.Geometry;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.DataStructures.Noise;
using GodotUtilities.DataStructures.Picker;
using GodotUtilities.GameData;

namespace HexGeneral.Game.Generators;

public static class MapGenerator
{
    private static int _chunkSize = 30;
    public static Map Generate(HexGeneralData data, NewGameSettings settings)
    {
        var map = Map.Create(data);
        MakeHexes(data, settings);
        DoLandSea(data, settings);
        DoLandforms(data, settings);
        DoVegetations(data, settings);
        MakeChunks(data, settings);
        MakeChunkUrbans(data, settings);
        MakeLandmasses(data, settings);
        MakeRoadNetwork(data, settings);

        return map;
    }

    private static void MakeHexes(HexGeneralData data, NewGameSettings settings)
    {
        var map = data.Map;
        var h = (int)settings.Height.Value;
        var w = (int)settings.Width.Value;
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
    private static void DoLandSea(HexGeneralData data, NewGameSettings settings)
    {
        var map = data.Map;
        var landSeaNoise = new TieredNoise();
        landSeaNoise.AddFastNoiseTier(new(-1f, 1f),
            .005f * settings.NoiseScale.Value);
        landSeaNoise.AddFastNoiseTier(new(-.5f, .5f),
            .01f * settings.NoiseScale.Value);
        landSeaNoise.AddFastNoiseTier(new(-.25f, .25f),
            .05f * settings.NoiseScale.Value);
        
        var landRatio = settings.LandRatio.Value;
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

    private static void DoLandforms(HexGeneralData data, NewGameSettings settings)
    {
        var map = data.Map;

        var isRoughNoise = new TieredNoise();
        isRoughNoise.AddFastNoiseTier(new(0f, 1f),
            .01f);
        var isRoughCutoff = settings.Roughness.Value;
        
        var roughnessNoise = new TieredNoise();
        roughnessNoise.AddFastNoiseTier(new(-.35f, .35f),
            .005f * settings.NoiseScale.Value);
        roughnessNoise.AddFastNoiseTier(new(-.25f, .5f),
            .01f * settings.NoiseScale.Value);
        roughnessNoise.AddFastNoiseTier(new(-.25f, 1f),
            .05f * settings.NoiseScale.Value);


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

    private static void DoVegetations(HexGeneralData data, NewGameSettings settings)
    {
        var map = data.Map;
        var moistureLevel = settings.Moisture.Value;
        var moistureNoise = new TieredNoise();
        moistureNoise.AddFastNoiseTier(new(-moistureLevel, moistureLevel * 2f),
            .003f * settings.NoiseScale.Value);
        moistureNoise.AddFastNoiseTier(
            new(-moistureLevel / 2f, moistureLevel / 2f),
            .01f * settings.NoiseScale.Value);

        var deforestNoise = new TieredNoise();
        var deforestCutoff = .6f;
        deforestNoise.AddFastNoiseTier(new Vector2(0f, 1f),
            .1f * settings.NoiseScale.Value);

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

    

    private static void MakeChunks(HexGeneralData data, NewGameSettings settings)
    {
        var map = data.Map;
        
        var hexChunks = new HexChunks(data.IdDispenser.TakeId(),
            new Dictionary<int, HexChunk>(),
            new Dictionary<Vector3I, int>());
        data.Entities.AddEntity(hexChunks, data);
        makeChunks(true);
        makeChunks(false);

        foreach (var chunk in hexChunks.Chunks.Values)
        {
            var neighborChunks = chunk.HexCoords
                .SelectMany(c =>
                    Hex.GetNeighborCoords(c, data)
                        .Select(n => hexChunks.HexChunkLookup[n]))
                .Distinct();
            foreach (var n in neighborChunks)
            {
                if (n != chunk.Id)
                {
                    chunk.NeighborChunkIds.Add(n);
                }
            }
        }        

        void makeChunks(bool isLand)
        {
            var hexes = map.Hexes.Values
                .Where(h => h.Landform.Get(data).IsLand == isLand)
                .ToHashSet();
            while (hexes.Count > 0)
            {
                var seed = hexes.First();
                var chunkHexes = FloodFill<Hex>
                    .FloodFillRandomishToLimit(seed, _chunkSize,
                        h =>
                        {
                            if (h is null) throw new Exception();
                            return h.GetNeighbors(data);
                        },
                        h => hexes.Contains(h),
                        data.Random);
                var chunk = new HexChunk(
                    chunkHexes.Select(h => h.Coords).ToHashSet(),
                    isLand, seed.Coords, data.IdDispenser.TakeId(),
                    new HashSet<int>());
                hexChunks.Chunks.Add(chunk.Id, chunk);
                foreach (var chunkHex in chunkHexes)
                {
                    hexChunks.HexChunkLookup.Add(chunkHex.Coords, chunk.Id);    
                }
                hexes.ExceptWith(chunkHexes);
            }
        }
    }

    private static void MakeChunkUrbans(HexGeneralData data, NewGameSettings settings)
    {
        var urban = data.ModelPredefs.Landforms.Urban;
        var barren = data.ModelPredefs.Vegetations.Barren;
        var chunks = data.HexChunks.Chunks;
        var map = data.Map;
        var exclude = new HashSet<HexChunk>();
        
        foreach (var (id, chunk) in chunks)
        {
            if (exclude.Contains(chunk)) continue;
            if (chunk.IsLand == false) continue;
            if (chunk.HexCoords.Count < 4) continue; 
            if (data.Random.Randf() < .75f) continue;
            var numUrbanHexes = data.Random.RandiRange(1, 5);
            numUrbanHexes = Mathf.Min(chunk.HexCoords.Count / 3, numUrbanHexes);
            var flood = FloodFill<Vector3I>
                .FloodFillRandomishToLimit(chunk.Seed, numUrbanHexes,
                    h => Hex.GetNeighborCoords(h, data),
                    h => chunk.HexCoords.Contains(h), 
                    data.Random);
            foreach (var coords in flood)
            {
                var hex = map.Hexes[coords];
                hex.SetLandform(urban.MakeIdRef(data));
                hex.SetVegetation(barren.MakeIdRef(data));
            }
            foreach (var nChunk in chunk.Neighbors(data))
            {
                exclude.Add(nChunk);
            }
        }
    }
    private static void MakeLandmasses(HexGeneralData data, NewGameSettings settings)
    {
        var map = data.Map;
        var chunks = data.HexChunks;
        var landHexes = map.Hexes.Values
            .Where(h => h.Landform.Get(data).IsLand)
            .ToHashSet();
        var seaHexes = map.Hexes.Values.Except(landHexes)
            .ToHashSet();

        var landUnions = UnionFind.Find<Hex, HashSet<Hex>>(
                landHexes, (t, r) => true, t => t.GetNeighbors(data))
            .Select(hs => hs.Select(h => h.Coords).ToHashSet())
            .ToList();
        
        var seaUnions = UnionFind.Find<Hex, HashSet<Hex>>(
                seaHexes, (t, r) => true, t => t.GetNeighbors(data))
            .Select(hs => hs.Select(h => h.Coords).ToHashSet())
            .ToList();
        var allUnions = landUnions.Concat(seaUnions).ToList();
        var dic = allUnions.ToDictionary(v => v, v => new HashSet<int>());
        
        foreach (var (id, chunk) in chunks.Chunks)
        {
            var union = allUnions.First(u => u.Contains(chunk.Seed));
            dic[union].Add(id);
        }
        var landmasses = new LandSeaMasses(data.IdDispenser.TakeId(),
            dic.Values.ToList());
        data.Entities.AddEntity(landmasses, data);
    }
    private static void MakeRoadNetwork(HexGeneralData data, NewGameSettings settings)
    {
        var map = data.Map;
        var urban = data.ModelPredefs.Landforms.Urban;
        var chunks = data.HexChunks;
        var landmasses = data.LandSeaMasses;
        
        foreach (var massChunkIds in landmasses.MassChunkIds)
        {
            var firstChunk = chunks.Chunks[massChunkIds.First()];
            if (firstChunk.IsLand == false) continue;
            var massChunks = massChunkIds
                .Select(id => chunks.Chunks[id]).ToArray();
            var urbanChunks = massChunks
                .Where(c => map.Hexes[c.Seed].Landform.Get(data) == urban);
                

            var greatChunkPicker = new Picker<HexChunk>(massChunks,
                h => h.Neighbors(data));
            foreach (var urbanSeed in urbanChunks)
            {
                greatChunkPicker.AddAgent(
                    new AdjacencyCountPickerAgent<HexChunk>(urbanSeed, greatChunkPicker, 1, h => true));
            }
            greatChunkPicker.RandomAgentPick();
            
            foreach (var agent in greatChunkPicker.Agents)
            {
                
            }
        }

        

        List<Hex> getPath(Hex start, Hex end)
        {
            if (start.WorldPos().DistanceTo(end.WorldPos()) > 20)
            {
                return new();
            }
            return PathFinder<Hex>.FindPath(
                start, end, v => v.GetNeighbors(data)
                    .Where(n => n.Landform.Get(data).IsLand),
                (v, w) =>
                {
                    var vLf = v.Landform.Get(data);
                    var wLf = w.Landform.Get(data);
                    return 1f + vLf.MinRoughness + wLf.MinRoughness;
                },
                (v, w) => v.WorldPos().DistanceTo(w.WorldPos()));
        }
        
    }
}