using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;

namespace HexGeneral.Game.Client.Graphics;

public partial class UnitGraphics : Node2D
{
    public Dictionary<ERef<Unit>, UnitGraphic> Graphics { get; private set; }
    public static Vector2[][] GraphicOffsetsByNumUnits { get; private set; } = new Vector2[][] { new[] { Vector2.Zero }, new[] { Vector2.Left / 2f, Vector2.Right / 2f }, new[] { Vector2.Up * HexExt.HexHeight / 2f, Vector2.Up.Rotated(2f * Mathf.Pi / 3f)  * HexExt.HexHeight / 2f, Vector2.Up.Rotated(4f * Mathf.Pi / 3f)  * HexExt.HexHeight / 2f, }, };

    public UnitGraphics(HexGeneralData data)
    {
        Graphics = new Dictionary<ERef<Unit>, UnitGraphic>();
        Update(data);
    }

    public void Update(HexGeneralData data)
    {
        var holder = data.MapUnitHolder;
        var units = holder.UnitPositions.Keys.ToHashSet();
        var obsolete = Graphics.Keys.Except(units).ToArray();
        var needGraphic = units.Except(Graphics.Keys).ToArray();
        foreach (var eRef in obsolete)
        {
            var graphic = Graphics[eRef];
            graphic.QueueFree();
            Graphics.Remove(eRef);
        }

        foreach (var eRef in needGraphic)
        {
            var unit = eRef.Get(data);
            var graphic = new UnitGraphic(unit, data);
            AddChild(graphic);
            Graphics.Add(eRef, graphic);
        }
        
        foreach (var (coords, hexUnits) in holder.HexLandUnits)
        {
            if (hexUnits.Count == 0) continue;
            var hexPos = coords.GetWorldPos();
            var offsets = GraphicOffsetsByNumUnits[hexUnits.Count - 1];
            for (var i = 0; i < hexUnits.Count; i++)
            {
                var unit = hexUnits[i];
                var graphic = Graphics[unit];
                graphic.Position = offsets[i] + hexPos;
                graphic.Update(unit.Get(data), data);
            }
        }
    }
}