using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class Location(int id, Vector3I coords, List<ModelIdRef<BuildingModel>> buildings) 
    : Entity(id)
{
    public Vector3I Coords { get; private set; } = coords;
    public List<ModelIdRef<BuildingModel>> Buildings { get; private set; } = buildings;

    public override void Made(Data d)
    {
        d.GetSingleton<LocationHolder>().Locations.Add(coords, this.MakeRef());
    }

    public override void CleanUp(Data d)
    {
        throw new System.Exception();
    }
}