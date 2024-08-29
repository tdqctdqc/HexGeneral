using System.Collections.Generic;
namespace HexGeneral.Game;
using GodotUtilities.DataStructures.AggregateTree;

public class NewGameData(NewGameSettings settings)
{
    public NewGameSettings Settings { get; private set; } = settings;
    public List<Branch<Hex>> HexBranches { get; private set; } = new List<Branch<Hex>>();
}