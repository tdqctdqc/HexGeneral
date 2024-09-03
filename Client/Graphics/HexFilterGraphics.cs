using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.Graphics;

namespace HexGeneral.Game.Client.Graphics;

public partial class HexFilterGraphics<TElement> : Node2D
{
    public static float TextureGameDimension { get; private set; }
        = 20f;

    public HexFilterGraphics(HexGeneralData data,
        Func<Hex, TElement> getElement,
        Func<TElement, Texture2D> getTexture,
        Func<Hex, Color> getColor)
    {
        var hexes = data.Map.Hexes.Values.ToArray();
        var elements = hexes.ToDictionary(h => h, h => getElement(h));

        var hexPs = ShapeBuilder.GetHex(Vector2.Zero, 1f)
            .ToArray();
        var unions = UnionFind.Find<Hex, List<Hex>>(
            hexes, (h, g) => elements[h].Equals(elements[g]),
            h => h.GetNeighbors(data)
        );

        foreach (var union in unions)
        {
            var element = elements[union[0]];
            var texture = getTexture(element);
            int iter = 0;
            var arrLength = union.Count * hexPs.Length;
            var vertices = new Vector2[arrLength];
            var uvs = new Vector2[arrLength];
            var colors = new Color[arrLength];
            var textureOffset = new Vector2(1f, HexExt.HexHeight);
            foreach (var hex in union)
            {
                var color = getColor(hex);
                var worldPos = hex.WorldPos();
                for (var i = 0; i < hexPs.Length; i++)
                {
                    var offset = hexPs[i];
                    var vertexWorldPos = offset + worldPos;
                    vertices[iter] = vertexWorldPos;
                    var uv = (offset + worldPos + textureOffset) / TextureGameDimension;
                    uvs[iter] = uv;
                    colors[iter] = color;
                    iter++;
                }
            }

            var mesh = MeshGenerator.GetArrayMesh(vertices, uvs, colors);
            
            var mi = new MeshInstance2D();
            mi.Mesh = mesh;
            mi.Texture = texture;
            mi.TextureRepeat = TextureRepeatEnum.Mirror;
            AddChild(mi);
        }
    }

}