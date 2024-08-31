using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;
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
        _baseHexColors = GetHexMesh(
            h => GetTerrainColor(h, _data), _data);

        AddChild(_baseHexColors);
    }

    public override void _Process(double delta)
    {
        int toShow = InputExt.GetNumKeyPressed();

        if (toShow != -1 && toShow != _showing)
        {
            _showing = toShow;
            this.ClearChildren();
            _baseHexColors = GetHexMesh(
                h => GetDebugColor(toShow, h, _data), _data);
            AddChild(_baseHexColors);
        }
        else
        {
            if (Input.IsKeyPressed(Key.T) && _showing != -1)
            {
                this.ClearChildren();
                _showing = -1;
                _baseHexColors = GetHexMesh(
                    h => GetTerrainColor(h, _data), _data);

                AddChild(_baseHexColors);
            }
            else if (Input.IsKeyPressed(Key.R) 
                     && _showing != -2)
            {
                this.ClearChildren();
                _showing = -2;
                _baseHexColors = _baseHexColors = GetHexMesh(
                    h => GetRegimeColor(h, _data), _data);

                AddChild(_baseHexColors);
            }
        }
        
        
        
    }

    private static MeshInstance2D GetHexMesh(Func<Hex, Color> getColor,
        HexGeneralData data)
    {
        var mb = new MeshBuilder();
        
        var map = data.Map;
        foreach (var (coords, hex) in map.Hexes)
        {
            var worldPos = coords.CubeToGridCoords().GetWorldPos();
            var hexTris = ShapeBuilder.GetHex(worldPos, 1f);
            Color color = getColor(hex);
            mb.AddTris(hexTris, color);
        }

        return mb.GetMeshInstance();
    }

    private static Color GetTerrainColor(Hex hex, HexGeneralData data)
    {
        var worldPos = hex.Coords.CubeToGridCoords().GetWorldPos();
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
        return color;
    }
    
    private static Color GetRegimeColor(Hex hex, HexGeneralData data)
    {
        Color color;
        if (hex.Regime.Fulfilled())
        {
            color = hex.Regime.Get(data).RegimeModel.Get(data).Color;
        }
        else
        {
            return GetTerrainColor(hex, data);
        }

        return color.Darkened(data.Random.RandfRange(-ColorWobble,
                ColorWobble));
    }

    public static Color GetDebugColor(int i,
        Hex hex,
        HexGeneralData data)
    {
        return hex.Colors.Count > i ? hex.Colors[i] : Colors.Transparent;
    }
    
}