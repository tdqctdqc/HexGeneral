using GodotUtilities.Logic;

namespace HexGeneral.Game.Logic.Editor;

public abstract class EditorAction
{
    public abstract void Do(ProcedureKey key);
    public abstract EditorAction GetUndoAction();
}