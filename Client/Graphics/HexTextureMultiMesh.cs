using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.DataStructures.Sorter;
using GodotUtilities.Graphics;

namespace HexGeneral.Game.Client.Graphics;

public partial class HexTextureMultiMesh<TElement> : Node2D
{
    private Dictionary<TElement, MultiMeshInstance2D> _mmis;
    private Func<TElement, Texture2D> _getTexture;
    public HexTextureMultiMesh(HexGeneralData data,
        Func<Hex,TElement> getElement, 
        Func<TElement, Texture2D> getTexture,
        Func<Hex, Color> getColor)
    {
        _getTexture = getTexture;
        _mmis = new Dictionary<TElement, MultiMeshInstance2D>();
        var byElement = data.Map.Hexes.Values
            .SortBy<TElement, Hex>(getElement);
        
        var width = 2f;
        var height = HexExt.HexHeight * 2f;
        
        var hexPs = ShapeBuilder.GetHex(Vector2.Zero, 1f).ToArray();

        var uvs = hexPs
            .Select(v => new Vector2((v.X + width / 2f) / width,
                (v.Y + height / 2f) / height))
            .ToArray();
        foreach (var (element, hexes) in byElement)
        {
            var mmi = new MultiMeshInstance2D();
            var mm = new MultiMesh();
            mmi.Multimesh = mm;
            
            mm.Mesh = MeshGenerator.GetArrayMesh(hexPs,
                uvs);
            mm.UseColors = true;
            mm.InstanceCount = hexes.Count;
            var texture = _getTexture(element);
            mmi.Texture = texture;
            for (var i = 0; i < hexes.Count; i++)
            {
                var hex = hexes[i];
                var color = getColor(hex);

                mm.SetInstanceColor(i, color);
                mm.SetInstanceTransform2D(i, new Transform2D(0f, hex.WorldPos()));
            }
            AddChild(mmi);
            _mmis.Add(element, mmi);
        }
    }
    
}