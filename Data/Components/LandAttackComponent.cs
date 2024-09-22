using Godot;
using GodotUtilities.Logic;
using HexGeneral.Game.Client;

namespace HexGeneral.Game.Components;

public class LandAttackComponent : IAttackComponent
{
    public Control GetDisplay(HexGeneralClient client)
    {
        var l = new Label();
        return l;
    }

    public void TurnTick(ProcedureKey key)
    {
        
    }

    public void Added(ProcedureKey key)
    {
    }

    public void Removed(ProcedureKey key)
    {
    }
}