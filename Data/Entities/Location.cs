using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class Location(int id, HexRef hex, List<ModelIdRef<BuildingModel>> buildings) 
    : Entity(id)
{
    public HexRef Hex { get; private set; } = hex;
    public List<ModelIdRef<BuildingModel>> Buildings { get; private set; } = buildings;

    public override void Made(GodotUtilities.GameData.Data d)
    {
        d.GetSingleton<LocationHolder>().Locations.Add(hex, this.MakeRef());
    }

    public override void CleanUp(GodotUtilities.GameData.Data d)
    {
        throw new System.Exception();
    }

    public bool HasBuilding<T>(HexGeneralData data)
        where T : BuildingModel
    {
        return Buildings.Any(b => b.Get(data) is T);
    }
}