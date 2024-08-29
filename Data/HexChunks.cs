using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class HexChunks(int id, Dictionary<int, HexChunk> chunks, Dictionary<Vector3I, int> hexChunkLookup) : Entity(id)
{
    public Dictionary<int, HexChunk> Chunks { get; private set; } = chunks;
    public Dictionary<Vector3I, int> HexChunkLookup { get; private set; } = hexChunkLookup;

    public override void Made(Data d)
    {
        d.SetEntitySingleton<HexChunks>();
    }

    public override void CleanUp(Data d)
    {
        throw new Exception();
    }
}