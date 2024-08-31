using System.Collections.Generic;
namespace HexGeneral.Game;
using GodotUtilities.DataStructures.AggregateTree;

public class NewGameData(NewGameSettings settings)
{
    public NewGameSettings Settings { get; private set; } = settings;
    public List<Branch<Hex>> TopBranches { get; private set; } = new List<Branch<Hex>>();
    public List<Branch<Hex>> UrbanTrunks { get; private set; } = new List<Branch<Hex>>();
    public List<Branch<Hex>> Twigs { get; private set; } = new List<Branch<Hex>>();
}