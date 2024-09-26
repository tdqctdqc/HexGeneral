using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class LocationHolder(int id, Dictionary<HexRef, ERef<Location>> locations) : Entity(id)
{
    public Dictionary<HexRef, ERef<Location>> Locations { get; private set; } = locations;

    public override void Made(GodotUtilities.GameData.Data d)
    {
        d.SetEntitySingleton<LocationHolder>();
    }

    public override void CleanUp(GodotUtilities.GameData.Data d)
    {
        throw new System.Exception();
    }
}