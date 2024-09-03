// using System.Collections.Generic;
// using System.Linq;
// using Godot;
//
// namespace HexGeneral.Game;
//
// public class HexChunk(HashSet<Vector3I> hexCoords, bool isLand, 
//     Vector3I seed, int id,
//     HashSet<int> neighborChunkIds)
// {
//     public int Id { get; private set; } = id;
//     public Vector3I Seed { get; private set; } = seed;
//     public bool IsLand { get; private set; } = isLand;
//     public HashSet<Vector3I> HexCoords { get; private set; } = hexCoords;
//     public HashSet<int> NeighborChunkIds { get; private set; } = neighborChunkIds;
//
//     
//     public Color GreatChunkColor { get; set; } = Colors.Transparent;
//     
//     
//     public IEnumerable<Hex> Hexes(HexGeneralData data)
//     {
//         return HexCoords.Select(h => data.Map.Hexes[h]);
//     }
//
//     public IEnumerable<HexChunk> Neighbors(HexGeneralData data)
//     {
//         return NeighborChunkIds.Select(i => data.HexChunks.Chunks[i]);
//     }
// }