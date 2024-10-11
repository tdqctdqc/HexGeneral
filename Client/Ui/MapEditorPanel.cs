using Godot;
using GodotUtilities.Ui;
using HexGeneral.Game;

namespace HexGeneral.Client.Ui;

public partial class MapEditorPanel : PanelContainer
{
    public MapEditorPanel(MapEditorMode mode, HexGeneralData data)
    {
        var vbox = new VBoxContainer();
        AddChild(vbox);
        var top = new VBoxContainer();
        vbox.AddChild(top);
        top.AddChild(mode.BrushSize.GetControlInterface());
        Button undo = null;
        undo = vbox.AddButton("Undo", () =>
        {
            mode.UndoEditorAction();
            undo.Disabled = mode.EditorActions.Count == 0;
        });
        mode.DidEditorAction += () =>
        {
            undo.Disabled = mode.EditorActions.Count == 0;
        };
        undo.Disabled = mode.EditorActions.Count == 0;
        var p = new MapEditorTabPanel(mode, data);
        p.ExpandFill();
        vbox.AddChild(p);
    }

}