using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.Logic;
using HexGeneral.Game.Client;

namespace HexGeneral.Game.Components;

public class MoveCountComponent(int movesTaken, int maxMoves, float movePointRatioRemaining) : IEntityComponent
{
    public float MovePointRatioRemaining { get; private set; } = movePointRatioRemaining;
    public int MovesTaken { get; private set; } = movesTaken;
    public int MaxMoves { get; private set; } = maxMoves;

    public Control GetDisplay(GameClient client)
    {
        var l = new Label();
        l.Text = $"Moves left: {MaxMoves - MovesTaken} / {MaxMoves}";
        return l;
    }

    public void TurnTick(ProcedureKey key)
    {
        MovePointRatioRemaining = 1f;
        MovesTaken = 0;
    }

    public void Added(ProcedureKey key)
    {
    }

    public void Removed(ProcedureKey key)
    {
        
    }

    public void SpendMove(float ratioSpent, ProcedureKey key)
    {
        MovePointRatioRemaining -= ratioSpent;
        MovesTaken++;
    }

    public bool HasMoved()
    {
        return MovesTaken > 0;
    }
    public bool CanMove()
    {
        return MovesTaken < MaxMoves && MovePointRatioRemaining > 0f;
    }
}