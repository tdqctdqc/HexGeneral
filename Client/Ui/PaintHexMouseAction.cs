using System;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameClient;
using GodotUtilities.Graphics;
using GodotUtilities.Server;
using GodotUtilities.Ui;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Game.Logic.Editor;

namespace HexGeneral.Client.Ui;

public class PaintHexMouseAction<T> : MouseAction
{
    private FloatSettingsOption _brushSize;
    private SettingsOption<T> _setting;
    private Func<T, Hex, EditorAction> _editorAction;
    private Action<EditorAction> _submitAction;
    private MapOverlayDrawer _brushOverlay;
    private HexGeneralClient _client;
    private Hex _cachedHex;
    private T _cachedSetting;
    
    public PaintHexMouseAction(MouseButtonMask button, 
        FloatSettingsOption brushSize, SettingsOption<T> setting, 
        Func<T, Hex, EditorAction> editorAction,
        
        HexGeneralClient client, Action<EditorAction> submitAction) : base(button)
    {
        _brushSize = brushSize;
        _setting = setting;
        _editorAction = editorAction;
        _client = client;
        _submitAction = submitAction;
        _brushOverlay = new MapOverlayDrawer((int)GraphicsLayers.Debug,
            () => client.GetComponent<MapGraphics>().Node);
        
    }

    protected override void MouseDown(InputEventMouse m)
    {
        
    }

    protected override void ProcessSub(InputEventMouse m)
    {
        _brushOverlay.Clear();

        var mousePos = _client.GetComponent<CameraController>().GetGlobalMousePosition();
        var hex = MouseOverHandler.FindMouseOverHex(_client);
        if (hex is null) return;
        var inRadius = hex.Coords.GetHexesInRadius((int)_brushSize.Value)
            .Select(c => _client.Data.Map.Hexes[c]);
        _brushOverlay.Draw(mb =>
        {
            foreach (var h in inRadius)
            {
                mb.DrawHex(h.WorldPos(), 1f, Colors.White.Tint(.25f));
            }
        }, Vector2.Zero);
    }

    
    protected override void MouseHeld(InputEventMouse m)
    {
        if (_setting.Value.Equals(_cachedSetting) == false)
        {
            _cachedSetting = _setting.Value;
            _cachedHex = null;
        }
        
        var hex = MouseOverHandler.FindMouseOverHex(_client);

        if (hex is null)
        {
            _cachedHex = null;
            return;
        }

        if (hex != _cachedHex)
        {
            _cachedHex = hex;
            var inRadius = hex.Coords.GetHexesInRadius((int)_brushSize.Value)
                .Select(c => _client.Data.Map.Hexes[c]);

            var actions = inRadius
                .Select(h => _editorAction(_setting.Value, h))
                .ToList();
            _submitAction(new AggregateEditorAction(actions));
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