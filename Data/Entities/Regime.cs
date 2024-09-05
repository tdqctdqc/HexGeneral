using System.Collections.Generic;
using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class Regime(int id, ModelIdRef<RegimeModel> regimeModel, float recruits, float industrialPoints, HashSet<Vector3I> hexes) : Entity(id)
{
    public ModelIdRef<RegimeModel> RegimeModel { get; private set; } = regimeModel;
    public HashSet<Vector3I> Hexes { get; private set; } = hexes;
    public float Recruits { get; private set; } = recruits;
    public float IndustrialPoints { get; private set; } = industrialPoints;
    
    public override void Made(Data d)
    {
    }

    public override void CleanUp(Data d)
    {
    }

    public void AddRecruits(float recruits)
    {
        Recruits += recruits;
    }
}