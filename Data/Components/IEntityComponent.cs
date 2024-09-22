using Godot;
using GodotUtilities.Logic;
using HexGeneral.Game.Client;

namespace HexGeneral.Game.Components;

public interface IEntityComponent
{
    Control GetDisplay(HexGeneralClient client);
    void TurnTick(ProcedureKey key);
    void Added(ProcedureKey key);
    void Removed(ProcedureKey key);
}