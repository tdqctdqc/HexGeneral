using System.Collections.Generic;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.Graphics;

namespace HexGeneral.Game.Client.Graphics;

public partial class HexGraphics : Node2D
{
    private MeshInstance2D _baseHexColors;
    private static float ColorWobble = .05f;
    public HexGraphics(HexGeneralData data)
    {
        _baseHexColors = DoTerrainColor(data);
        AddChild(_baseHexColors);

        // var chunks = DoChunks(data);
        // AddChild(chunks);
        
        // var greatChunks = DoGreatChunks(data);
        // AddChild(greatChunks);
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
    private static MeshInstance2D DoChunks(HexGeneralData data)
    {
        var mb = new MeshBuilder();
        
        var map = data.Map;
        var chunks = data.HexChunks.Chunks;
        foreach (var (id, chunk) in chunks)
        {
            var color = ColorsExt.GetRandomColor();
            foreach (var coords in chunk.HexCoords)
            {
                var worldPos = coords.CubeToGridCoords().GetWorldPos();
                var hexTris = ShapeBuilder.GetHex(worldPos, 1f);
                mb.AddTris(hexTris, color);
            }
            
        }

        return mb.GetMeshInstance();
    }
    private static MeshInstance2D DoGreatChunks(HexGeneralData data)
    {
        var mb = new MeshBuilder();
        
        var map = data.Map;
        var chunks = data.HexChunks.Chunks;
        foreach (var (id, chunk) in chunks)
        {
            var color = chunk.GreatChunkColor;
            foreach (var coords in chunk.HexCoords)
            {
                var worldPos = coords.CubeToGridCoords().GetWorldPos();
                var hexTris = ShapeBuilder.GetHex(worldPos, 1f);
                mb.AddTris(hexTris, color);
            }
            
        }

        return mb.GetMeshInstance();
    }
}