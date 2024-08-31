using System.Collections.Generic;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.Graphics;
using GodotUtilities.Input;

namespace HexGeneral.Game.Client.Graphics;

public partial class HexGraphics : Node2D
{
    private MeshInstance2D _baseHexColors;
    private static float ColorWobble = .05f;
    private int _showing = 0;
    private HexGeneralData _data;
    public HexGraphics(HexGeneralData data)
    {
        _data = data;
        _baseHexColors = DoTerrainColor(data);
        AddChild(_baseHexColors);
    }

    public override void _Process(double delta)
    {
        int toShow = InputExt.GetNumKeyPressed();

        if (toShow != -1 && toShow != _showing)
        {
            _showing = toShow;
            this.ClearChildren();
            if (_showing == 0)
            {
                _baseHexColors = DoTerrainColor(_data);
                AddChild(_baseHexColors);
            }
            else
            {
                _baseHexColors = DoChunkColor(_showing - 1, _data);
                AddChild(_baseHexColors);
            }
        }
    }

    private static MeshInstance2D DoTerrainColor(HexGeneralData data)
    {
        var mb = new MeshBuilder();
        
        var map = data.Map;
        foreach (var (coords, hex) in map.Hexes)
        {
            var worldPos = coords.CubeToGridCoords().GetWorldPos();
            var hexTris = ShapeBuilder.GetHex(worldPos, 1f);

            var v = hex.Vegetation.Get(data);

            Color color;
            if (v.Color == Colors.Transparent)
            {
                color = hex.Landform.Get(data).Color;
            }
            else
            {
                color = v.Color.Darkened(hex.Landform.Get(data).DarkenFactor);
            }

            color = color.Darkened(data.Random.RandfRange(-ColorWobble,
                ColorWobble));
            mb.AddTris(hexTris, color);
        }

        return mb.GetMeshInstance();
    }
    private static MeshInstance2D DoChunkColor(int i, HexGeneralData data)
    {
        var mb = new MeshBuilder();
        
        var map = data.Map;
        foreach (var (coords, hex) in map.Hexes)
        {
            var worldPos = coords.CubeToGridCoords().GetWorldPos();
            var hexTris = ShapeBuilder.GetHex(worldPos, 1f);

            var color = hex.Colors.Count > i ? hex.Colors[i] : Colors.Transparent;
            mb.AddTris(hexTris, color);
        }
    
        return mb.GetMeshInstance();
    }
    
}