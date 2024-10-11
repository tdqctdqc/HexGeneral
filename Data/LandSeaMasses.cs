using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class LandSeaMasses(int id, List<HashSet<int>> massChunkIds) 
    : Entity(id), ISingletonEntity
{
    public List<HashSet<int>> MassChunkIds { get; private set; } = massChunkIds;

    public override void Made(GodotUtilities.GameData.Data d)
    {
    }

    public override void CleanUp(GodotUtilities.GameData.Data d)
    {
    }
}