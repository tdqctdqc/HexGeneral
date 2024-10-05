using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using HexGeneral.Client.Ui;
using HexGeneral.Game.Components;

namespace HexGeneral.Game.Client.Graphics;

public partial class UnitGraphics : Node2D
{
    public Dictionary<ERef<Unit>, UnitGraphic> Graphics { get; private set; }
    public static Vector2[][] GraphicOffsetsByNumUnits { get; private set; } = new Vector2[][] { new[] { Vector2.Zero }, new[] { Vector2.Left / 2f, Vector2.Right / 2f }, new[] { Vector2.Up * HexExt.HexHeight / 2f, Vector2.Up.Rotated(2f * Mathf.Pi / 3f)  * HexExt.HexHeight / 2f, Vector2.Up.Rotated(4f * Mathf.Pi / 3f)  * HexExt.HexHeight / 2f, }, };
    public Dictionary<HexRef, HexUnitGraphicsHolder> HexGraphicsHolders { get; private set; }
    public List<Domain> DomainsByVisibilityPriority { get; private set; }
    public UnitGraphics(HexGeneralClient client)
    {
        ZIndex = (int)GraphicsLayers.Units;
        ZAsRelative = false;
        Graphics = new Dictionary<ERef<Unit>, UnitGraphic>();
        HexGraphicsHolders = new Dictionary<HexRef, HexUnitGraphicsHolder>();
        DomainsByVisibilityPriority = new List<Domain>();
        foreach (var domain in client.Data.Models.GetModels<Domain>())
        {
            DomainsByVisibilityPriority.Add(domain);
        }
        
        Update(client);
    }
    
    public void Update(HexGeneralClient client)
    {
        var holder = client.Data.MapUnitHolder;
        var units = holder.UnitPositions.Keys.ToHashSet();
        var obsolete = Graphics.Keys.Except(units).ToArray();
        foreach (var eRef in obsolete)
        {
            var graphic = Graphics[eRef];
            graphic.QueueFree();
            Graphics.Remove(eRef);
        }
        foreach (var hex in client.Data.Map.Hexes.Values)
        {
            GetHexHolder(hex, client.Data)?.DrawUnits(client);
        }
    }

    public UnitGraphic GetUnitGraphic(Unit unit, HexGeneralData data)
    {
        if (Graphics.TryGetValue(unit.MakeRef(), out var graphic))
        {
            return graphic;
        }

        graphic = new UnitGraphic(unit, data);
        Graphics.Add(unit.MakeRef(), graphic);
        AddChild(graphic);
        return graphic;
    }
    private HexUnitGraphicsHolder GetHexHolder(Hex hex, HexGeneralData data)
    {
        var hRef = hex.MakeRef();
        if (hex.GetAllUnits(data).Any())
        {
            if (HexGraphicsHolders.TryGetValue(hRef, 
                    out var holder))
            {
                return holder;
            }
            holder = HexUnitGraphicsHolder.Construct(this, hex);
            AddChild(holder);
            HexGraphicsHolders.Add(hex.MakeRef(), holder);
            return holder;
        }
        if(HexGraphicsHolders.TryGetValue(hRef, out var h))
        {
            h.QueueFree();
            HexGraphicsHolders.Remove(hRef);
        }
        return null;
    }
    public void UpdateUnit(Unit u, HexGeneralClient client)
    {
        Graphics[u.MakeRef()].Update(u, client);
    }
    public void DrawHex(HexRef hex, HexGeneralClient client)
    {
        GetHexHolder(hex.Get(client.Data), client.Data)?.DrawUnits(client);
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

    public Unit GetClosestUnitInHex(Hex hex,
        Vector2 worldPos, HexGeneralClient client)
    {
        if (HexGraphicsHolders.TryGetValue(hex.MakeRef(), out var holder))
        {
            return holder.GetClosestUnitInHex(worldPos, client);
        }

        return null;
    }

    public void CycleDomainPriority(HexGeneralClient client)
    {
        var first = DomainsByVisibilityPriority[0];
        DomainsByVisibilityPriority.RemoveAt(0);
        DomainsByVisibilityPriority.Add(first);
        foreach (var (key, value) in HexGraphicsHolders)
        {
            value.UpdateVisibility(client);
        }
    }
}