using GodotUtilities.Logic;
using HexGeneral.Game.Logic.Editor;

namespace HexGeneral.Logic.Procedures;

public class EditorActionProcedure(EditorAction action) : GodotUtilities.Server.Procedure
{
    public EditorAction Action { get; private set; } = action;

    public override void Handle(ProcedureKey key)
    {
        Action.Do(key);
    }
}