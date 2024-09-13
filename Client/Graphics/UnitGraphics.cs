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
    public UnitGraphics(HexGeneralClient client)
    {
        Graphics = new Dictionary<ERef<Unit>, UnitGraphic>();
        Update(client);

        var unitMode = client.UiController.ModeOption.Options.OfType<UnitMode>().First();
        unitMode.SelectedUnit.SettingChanged.SubscribeForNode(v =>
        {
            if (v.oldVal is Unit oldU && Graphics.TryGetValue(oldU.MakeRef(), out var oldGraphic))
            {
                oldGraphic.SetHighlight(false);
            }
            if (v.newVal is Unit newU && Graphics.TryGetValue(newU.MakeRef(), out var newGraphic))
            {
                newGraphic.SetHighlight(true);
            }
        }, this);

        unitMode.Exited += () =>
        {
            if (unitMode.SelectedUnit.Value is Unit unit
                && Graphics.TryGetValue(unit.MakeRef(), out var graphic))
            {
                graphic.SetHighlight(false);
            }
        };
    }
    
    public void Update(HexGeneralClient client)
    {
        var holder = client.Data.MapUnitHolder;
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
            var unit = eRef.Get(client.Data);
            var graphic = new UnitGraphic(unit, client.Data);
            AddChild(graphic);
            Graphics.Add(eRef, graphic);
        }
        
        foreach (var (r, hexUnits) in holder.HexLandUnits)
        {
            DrawHex(r, client);
        }
    }

    public void UpdateUnit(Unit u, HexGeneralClient client)
    {
        Graphics[u.MakeRef()].Update(u, client);
    }
    public void DrawHex(HexRef hex, HexGeneralClient client)
    {
        var hexUnits = client.Data.MapUnitHolder.HexLandUnits[hex];
        
        if (hexUnits.Count == 0) return;
        var hexPos = hex.GetWorldPos();
        var offsets = GraphicOffsetsByNumUnits[hexUnits.Count - 1];
        for (var i = 0; i < hexUnits.Count; i++)
        {
            var unit = hexUnits[i];
            var graphic = Graphics[unit];
            graphic.Position = offsets[i] + hexPos;
            graphic.Update(unit.Get(client.Data), client);
        }
    }

    public void RemoveUnit(Unit u, Hex hex, HexGeneralClient client)
    {
        if (hex is null) return;

        var r = u.MakeRef();
        var graphic = Graphics[r];
        Graphics.Remove(r);
        graphic.QueueFree();
        DrawHex(hex.MakeRef(), client);
    }
}