using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using GodotUtilities.Input;

namespace HexGeneral.Game.Client.Graphics;

public partial class HexBaseColorGraphics : Node2D
{
    private HexColorMesh<Hex> _baseHexColors;
    public static float ColorWobble = .05f;
    private int _showing = Int32.MinValue;
    private HexGeneralData _data;
    public HexBaseColorGraphics(HexGeneralData data)
    {
        _data = data;
        _baseHexColors = new HexColorMesh<Hex>(data.Map.Hexes.Values.ToList(),
            h => h.WorldPos(), 1f);
        AddChild(_baseHexColors);
        SetBaseColor(h => h.GetTerrainColor(_data));
    }
    
    public override void _Process(double delta)
    {
        int toShow = InputExt.GetNumKeyPressed();

        if (toShow != _showing && toShow >= 0)
        {
            _showing = toShow;
            SetBaseColor(h => GetDebugColor(toShow, h));
        }
        else
        {
            if (Input.IsKeyPressed(Key.T) && _showing != -1)
            {
                _showing = -1;
                SetBaseColor(h => h.GetTerrainColor(_data));
            }
            else if (Input.IsKeyPressed(Key.R) 
                     && _showing != -2)
            {
                _showing = -2;
                SetBaseColor(h => h.GetTerrainColor(_data));
            }
        }
    }

    private void SetBaseColor(Func<Hex, Color> getColor)
    {
        _baseHexColors.SetColors(getColor);
    }

    
    

    public static Color GetDebugColor(int i, Hex hex)
    {
        return i > 0 && hex.DebugColors.Count > i - 1 ? hex.DebugColors[i - 1] : Colors.Transparent;
    }
    
}