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
    public Dictionary<Domain, Node2D> DomainNodes { get; private set; }
    public static Vector2[][] GraphicOffsetsByNumUnits { get; private set; } = new Vector2[][] { new[] { Vector2.Zero }, new[] { Vector2.Left / 2f, Vector2.Right / 2f }, new[] { Vector2.Up * HexExt.HexHeight / 2f, Vector2.Up.Rotated(2f * Mathf.Pi / 3f)  * HexExt.HexHeight / 2f, Vector2.Up.Rotated(4f * Mathf.Pi / 3f)  * HexExt.HexHeight / 2f, }, };
    public UnitGraphics(HexGeneralClient client)
    {
        ZIndex = (int)GraphicsLayers.Units;
        ZAsRelative = false;
        Graphics = new Dictionary<ERef<Unit>, UnitGraphic>();
        DomainNodes = new Dictionary<Domain, Node2D>();
        foreach (var domain in client.Data.Models.GetModels<Domain>())
        {
            var n = new Node2D();
            AddChild(n);
            DomainNodes.Add(domain, n);
        }
        
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
            var domain = unit.Components.Get<IMoveComponent>(client.Data)
                .GetActiveMoveType(client.Data).Domain;
            DomainNodes[domain].AddChild(graphic);
            Graphics.Add(eRef, graphic);
        }
        foreach (var (key, value) in holder.HexUnitsByDomain)
        {
            foreach (var (r, hexUnits) in value)
            {
                DrawHex(r, client);
            }
        }
    }

    public void UpdateUnit(Unit u, HexGeneralClient client)
    {
        Graphics[u.MakeRef()].Update(u, client);
    }
    public void DrawHex(HexRef hex, HexGeneralClient client)
    {
        foreach (var (domain, dic) in client.Data.MapUnitHolder.HexUnitsByDomain)
        {
            var node = DomainNodes[domain.Get(client.Data)];
            if (dic.ContainsKey(hex) == false) continue;
            var hexUnits = dic[hex];
            if (hexUnits.Count == 0) continue;
            var hexPos = hex.GetWorldPos();
            var offsets = GraphicOffsetsByNumUnits[hexUnits.Count - 1];
            for (var i = 0; i < hexUnits.Count; i++)
            {
                var unit = hexUnits[i];
                if (Graphics.ContainsKey(unit) == false)
                {
                    var g = new UnitGraphic(unit.Get(client.Data), client.Data);
                    node.AddChild(g);
                    Graphics.Add(unit, g);
                }
                else if (Graphics[unit].GetParent() != node)
                {
                    
                }
                
                var graphic = Graphics[unit];
                graphic.Position = offsets[i] + hexPos;
                graphic.Update(unit.Get(client.Data), client);
            }
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

    public Unit GetClosestUnitInHex(Hex hex,
        Vector2 worldPos, HexGeneralClient client)
    {
        foreach (var (domain, node) in DomainNodes)
        {
            if (node.Visible == false) continue;
            var dic = client.Data.MapUnitHolder.HexUnitsByDomain[domain.MakeIdRef(client.Data)];
        
            if (dic.ContainsKey(hex.MakeRef()) == false) continue;
            var unitsInTargetHex = dic[hex.MakeRef()];
        
            if (unitsInTargetHex.Count == 0)
            {
                continue;
            }

            var offsets = GraphicOffsetsByNumUnits[unitsInTargetHex.Count - 1];
            var close = offsets.IndexOf(offsets.MinBy(o =>
            {
                return (o + hex.WorldPos()).DistanceSquaredTo(worldPos);
            }));
            var targetUnit = unitsInTargetHex[close];
            return targetUnit.Get(client.Data);
        }

        return null;
    }
}