using System;
using Godot;
using GodotUtilities.DataStructures.Hex;

namespace HexGeneral.Game.Client.Graphics;

public partial class MapInputCatcher : Control
{
    public Action<InputEvent> Input { get; set; }

    public MapInputCatcher(Vector2I bounds)
    {
        Size = new Vector2((bounds.X + 1) * 1.5f, 
            (bounds.Y + 1) * HexExt.HexHeight * 2f);
        Position = new Vector2(-1f, -HexExt.HexHeight);
    }

    public override void _GuiInput(InputEvent e)
    {
        Input?.Invoke(e);
    }
}