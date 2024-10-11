using System.Collections.Generic;
using System.Linq;
using GodotUtilities.Logic;

namespace HexGeneral.Game.Logic.Editor;

public class AggregateEditorAction(List<EditorAction> actions) : EditorAction
{
    public List<EditorAction> Actions { get; private set; } = actions;

    public override void Do(ProcedureKey key)
    {
        for (var i = 0; i < Actions.Count; i++)
        {
            Actions[i].Do(key);
        }
    }

    public override EditorAction GetUndoAction()
    {
        return new AggregateEditorAction(
            Actions.AsEnumerable().Reverse()
                .Select(a => a.GetUndoAction()).ToList());
    }
}