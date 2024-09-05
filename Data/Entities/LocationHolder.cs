using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class LocationHolder(int id, Dictionary<Vector3I, ERef<Location>> locations) : Entity(id)
{
    public Dictionary<Vector3I, ERef<Location>> Locations { get; private set; } = locations;

    public override void Made(Data d)
    {
        d.SetEntitySingleton<LocationHolder>();
    }

    public override void CleanUp(Data d)
    {
        throw new System.Exception();
    }
}