using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.Graphics;

namespace HexGeneral.Game.Client.Graphics;

public partial class HexBorderMultiMesh<TElement> : MultiMeshInstance2D
{
    private Dictionary<Vector2I, int> _edgeIndices;
    private Func<Hex, TElement> _getElement;
    private Func<TElement, Color> _getColor; 
    public HexBorderMultiMesh(Func<TElement, Color> getColor,
        Func<Hex, TElement> getElement, HexGeneralClient client)
    {
        _getElement = getElement;
        _getColor = getColor;
        var hexBorderShape = ShapeBuilder
            .GetHexBorder(Vector3I.Zero, HexExt.North,
                1f, .2f)
            .Select(p => p + HexExt.HexHeight * Vector2.Up);
        
        var mesh = MeshGenerator.GetArrayMesh(hexBorderShape.ToArray());
        Multimesh = new MultiMesh();
        Multimesh.UseColors = true;
        Multimesh.Mesh = mesh;
        
        _edgeIndices = new Dictionary<Vector2I, int>();
        var colors = new List<Color>();
        var transforms = new List<Transform2D>();
        var edges = new List<Vector2I>();
        foreach (var hex in client.Data.Map.Hexes.Values)
        {
            var element = _getElement(hex);
            var color = _getColor(element);
            var pos = hex.WorldPos();
            foreach (var nHex in hex.GetNeighbors(client.Data))
            {
                var nElement = _getElement(nHex);
                if (element is not null && element.Equals(nElement))
                {
                    colors.Add(Colors.Transparent);
                }
                else
                {
                    colors.Add(color);
                }
                
                var dir = nHex.Coords - hex.Coords;
                var angle = -HexExt.HexDirs.IndexOf(dir) * (Mathf.Pi / 3f);
                transforms.Add(new Transform2D(angle, pos));
                edges.Add(new Vector2I(hex.Id, nHex.Id));
            }
        }

        Multimesh.InstanceCount = colors.Count;
        for (var i = 0; i < colors.Count; i++)
        {
            var color = colors[i];
            var transform = transforms[i];
            Multimesh.SetInstanceColor(i, color);
            Multimesh.SetInstanceTransform2D(i, transform);
            _edgeIndices.Add(edges[i], i);
        }
    }

    public void Update(HexGeneralClient client)
    {
        foreach (var hex in client.Data.Map.Hexes.Values)
        {
            UpdateHex(hex, client);
        }
    }
    public void UpdateHex(Hex hex, HexGeneralClient client)
    {
        var element = _getElement(hex);
        var color = _getColor(element);
        foreach (var nHex in hex.GetNeighbors(client.Data))
        {
            var edge = new Vector2I(hex.Id, nHex.Id);
            var oppEdge = new Vector2I(nHex.Id, hex.Id);
            var index = _edgeIndices[edge];
            var oppIndex = _edgeIndices[oppEdge];
            var nElement = _getElement(nHex);
            
            var edgeColor = element is null || element.Equals(nElement)
                ? Colors.Transparent
                : color;
            var oppEdgeColor = nElement is null || nElement.Equals(element)
                ? Colors.Transparent
                : _getColor(nElement);
            Multimesh.SetInstanceColor(index, edgeColor);
            Multimesh.SetInstanceColor(oppIndex, oppEdgeColor);
        }
    }
}