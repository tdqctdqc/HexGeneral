using System;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.Graphics;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using Microsoft.VisualBasic;

namespace HexGeneral.Client.Ui;

public static class MouseOverHandler
{
    public static Hex FindMouseOverHex(HexGeneralClient client)
    {
        var pos = client.GetComponent<CameraController>().GetGlobalMousePosition();
        var mouseHex = MouseOverHandler.FindTwoClosestHexes(pos,
            client.Data.Map);
        return mouseHex.closest;
    }
    public static (Hex closest, Hex close) FindTwoClosestHexes(Vector2 worldPos,
        Map map)
    {
        var radius = 1f;
        var mapWidth = map.GridBounds.X;
        var mapHeight = map.GridBounds.Y;
        int x = (int)(worldPos.X / (radius * 1.5f));
        int y = (int)(worldPos.Y / (radius * .866f * 2f));
        
        float closestDist = Mathf.Inf; 
        Vector3I closestCoords = Vector3I.MinValue;

        float closeDist = Mathf.Inf;
        Vector3I closeCoords = Vector3I.MinValue;
        for (int i = x - 2; i < x + 2; i++)
        {
            for (int j = y - 2; j < y + 2; j++)
            {
                if(i >= 0 && i < mapWidth && j >= 0 && j < mapHeight)
                {
                    var coords = new Vector2I(i,j);
                    if (map.InGridBounds(coords) == false) continue;
                    Vector3I tempID = coords.GridCoordsToCube();
                    Vector2 tempPos = coords.GetWorldPos();
                    float tempDist = tempPos.DistanceTo(worldPos);
                    if(tempDist < closestDist)
                    {
                        closeDist = closestDist;
                        closeCoords = closestCoords;
                        closestDist = tempDist;
                        closestCoords = tempID;
                    }
                    else if(tempDist < closeDist)
                    {
                        closeDist = tempDist;
                        closeCoords = tempID;
                    }
                } 
            }
        }

        var closest = map.Hexes.TryGetValue(closestCoords, out var c1)
            ? c1
            : null;
        var close = map.Hexes.TryGetValue(closeCoords, out var c2)
            ? c2
            : null;
        return (closest, close);
    }
}