using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class LandSeaMasses(int id, List<HashSet<int>> massChunkIds) : Entity(id)
{
    public List<HashSet<int>> MassChunkIds { get; private set; } = massChunkIds;

    public override void Made(Data d)
    {
        d.SetEntitySingleton<LandSeaMasses>();
    }

    public override void CleanUp(Data d)
    {
    }
}