using System;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures.Sorter;
using GodotUtilities.Graphics;

namespace HexGeneral.Game.Client.Graphics;

public partial class HexFilterGraphicsTest<TElement> : Node2D
{
    
    private Func<TElement, Texture2D> _getTexture;
    public HexFilterGraphicsTest(HexGeneralData data,
        Func<Hex,TElement> getElement, Func<TElement, Texture2D> getTexture)
    {
        _getTexture = getTexture;
        var byElement = data.Map.Hexes.Values
            .SortBy<TElement, Hex>(getElement);
        
        foreach (var (element, hexes) in byElement)
        {
            foreach (var hex in hexes)
            {
                var hexPs = ShapeBuilder.GetHex(Vector2.Zero, 1f).ToArray();
            
                var mesh = MeshGenerator.GetArrayMesh(hexPs, 
                    hexPs.ToArray());
            
                var texture = _getTexture(element);
                var mi = new MeshInstance2D();
                mi.Texture = texture;
                mi.Mesh = mesh;
                mi.Position = hex.WorldPos();
                AddChild(mi);
            }
            
        }
    }
}