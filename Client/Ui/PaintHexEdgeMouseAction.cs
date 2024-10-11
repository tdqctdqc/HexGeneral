using System;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Game.Logic.Editor;

namespace HexGeneral.Client.Ui;

public class PaintHexEdgeMouseAction<T> : MouseAction
{
    private SettingsOption<T> _setting;
    private Func<T, Vector2I, EditorAction> _editorAction;
    private Action<EditorAction> _submitAction;
    private MapOverlayDrawer _brushOverlay;
    private HexGeneralClient _client;
    private bool _borderNotPath;
    private Vector2I _cachedEdge;
    private T _cachedSetting;
    
    public PaintHexEdgeMouseAction(MouseButtonMask button, 
        SettingsOption<T> setting, 
        Func<T, Vector2I, EditorAction> editorAction,
        
        HexGeneralClient client, Action<EditorAction> submitAction, bool borderNotPath) : base(button)
    {
        _setting = setting;
        _editorAction = editorAction;
        _client = client;
        _submitAction = submitAction;
        _borderNotPath = borderNotPath;
        _brushOverlay = new MapOverlayDrawer((int)GraphicsLayers.Debug,
            () => client.GetComponent<MapGraphics>().Node);
        
    }

    protected override void MouseDown(InputEventMouse m)
    {
        
    }

    protected override void ProcessSub(InputEventMouse m)
    {
        _brushOverlay.Clear();

        if (TryGetEdge(out var edge) == false) return;
        
        _brushOverlay.Draw(mb =>
        {
            var h1 = _client.Data.Map.Hexes[_client.Data.Map.CoordsById[edge.X]];
            var h2 = _client.Data.Map.Hexes[_client.Data.Map.CoordsById[edge.Y]];

            if (_borderNotPath)
            {
                mb.AddLine(h1.WorldPos(), h2.WorldPos(),
                    Colors.White.Tint(.25f), .25f);
            }
            else
            {
                var mid = (h1.WorldPos() + h2.WorldPos()) / 2f;
                var axis = h1.WorldPos() - h2.WorldPos();
                var perp = axis.Orthogonal().Normalized();
                var p1 = mid + perp / 2f;
                var p2 = mid - perp / 2f;
                mb.AddLine(p1, p2,
                    Colors.White.Tint(.25f), .25f);
            }
            
        }, Vector2.Zero);
    }

    private bool TryGetEdge(out Vector2I edge)
    {
        edge = Vector2I.MaxValue;
        var mousePos = _client.GetComponent<CameraController>().GetGlobalMousePosition();

        var (closest, close) = MouseOverHandler.FindTwoClosestHexes(
            mousePos,
            _client.Data.Map);
        if (close is null || closest is null) return false;
        edge = close.GetIdEdgeKey(closest);
        return true;
    }
    
    protected override void MouseHeld(InputEventMouse m)
    {
        if (_setting.Value.Equals(_cachedSetting) == false)
        {
            _cachedSetting = _setting.Value;
            _cachedEdge = Vector2I.MaxValue;
        }
        

        if (TryGetEdge(out var edge) is false)
        {
            _cachedEdge = Vector2I.MaxValue;
            return;
        }

        if (edge != _cachedEdge)
        {
            _cachedEdge = edge;
            var action = _editorAction(_setting.Value, _cachedEdge);
            _submitAction(action);
        }
    }

    protected override void MouseUp(InputEventMouse m)
    {
        
    }

    public override void Clear()
    {
        _brushOverlay.Clear();
    }
}