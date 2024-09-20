using Godot;
using GodotUtilities.Logic;
using HexGeneral.Game.Client;

namespace HexGeneral.Game.Components;

public interface IComponent
{
    Control GetDisplay(HexGeneralClient client);
    void Added(ProcedureKey key);
}