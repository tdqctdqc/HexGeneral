using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.GameData;
using HexGeneral.Game.Components;

namespace HexGeneral.Game.Client.Graphics;

public partial class HexUnitGraphicsHolder : Node2D
{
    private UnitGraphics _unitGraphics;
    private Hex _hex;

    public static HexUnitGraphicsHolder Construct(UnitGraphics unitGraphics, Hex hex)
    {
        var h = SceneManager.Instance<HexUnitGraphicsHolder>();
        h._unitGraphics = unitGraphics;
        h._hex = hex;
        h.Position = hex.WorldPos();
        return h;
    }

    public void DrawUnits(HexGeneralClient client)
    {
        var data = client.Data;
        var drawn = false;
        var unitHolder = data.MapUnitHolder.HexUnitsByDomain;
        
        
        for (var i = 0; i < _unitGraphics.DomainsByVisibilityPriority.Count; i++)
        {
            var domain = _unitGraphics.DomainsByVisibilityPriority[i];
            var box = (HBoxContainer)FindChild(domain.Name);
            box.Visible = false;

            if (unitHolder[domain.MakeIdRef(data)]
                    .TryGetValue(_hex.MakeRef(), out var domainUnits)
                        == false
                    || domainUnits.Count == 0)
            {
                continue;
            }
            var hexPos = _hex.WorldPos();
            
            var positions = UnitGraphics
                .GraphicOffsetsByNumUnits[domainUnits.Count - 1];
            for (var j = 0; j < domainUnits.Count; j++)
            {
                var domainUnit = domainUnits[j];
                var unitGraphic = _unitGraphics.GetUnitGraphic(domainUnit.Get(data), data);
                unitGraphic.Visible = drawn == false;
                unitGraphic.Update(domainUnit.Get(data), client);
                unitGraphic.Position = hexPos + positions[j];
            }
            
            if(domainUnits.Count > 0)
            {
                if (drawn)
                {
                    box.Visible = true;
                    ((Label)box.FindChild("Label")).Text = domainUnits.Count.ToString();
                }
                
                drawn = true;
            }
        }
    }

    public void UpdateVisibility(HexGeneralClient client)
    {
        var data = client.Data;
        var drawn = false;
        var unitHolder = data.MapUnitHolder.HexUnitsByDomain;
        
        for (var i = 0; i < _unitGraphics.DomainsByVisibilityPriority.Count; i++)
        {
            var domain = _unitGraphics.DomainsByVisibilityPriority[i];
            var box = (HBoxContainer)FindChild(domain.Name);
            box.Visible = false;

            if (unitHolder[domain.MakeIdRef(data)].ContainsKey(_hex.MakeRef()) == false)
            {
                continue;
            }
            var domainUnits
                = unitHolder[domain.MakeIdRef(data)][_hex.MakeRef()];
            for (var j = 0; j < domainUnits.Count; j++)
            {
                var domainUnit = domainUnits[j];
                var unitGraphic = _unitGraphics.GetUnitGraphic(domainUnit.Get(data), data);
                unitGraphic.Visible = drawn == false;
            }
            
            if(domainUnits.Count > 0)
            {
                if (drawn)
                {
                    box.Visible = true;
                    ((Label)box.FindChild("Label")).Text = domainUnits.Count.ToString();
                }
                drawn = true;
            }
        }
    }

    public Unit GetClosestUnitInHex(Vector2 worldPos, HexGeneralClient client)
    {
        for (var i = 0; i < _unitGraphics.DomainsByVisibilityPriority.Count; i++)
        {
            var domain = _unitGraphics.DomainsByVisibilityPriority[i];
            var dic = client.Data.MapUnitHolder.HexUnitsByDomain[domain.MakeIdRef(client.Data)];
            
            if (dic.ContainsKey(_hex.MakeRef()) == false) continue;
            var unitsInTargetHex = dic[_hex.MakeRef()];
        
            if (unitsInTargetHex.Count == 0)
            {
                continue;
            }

            var offsets = UnitGraphics.GraphicOffsetsByNumUnits[unitsInTargetHex.Count - 1];
            var close = offsets.IndexOf(offsets.MinBy(o =>
            {
                return (o + _hex.WorldPos()).DistanceSquaredTo(worldPos);
            }));
            var targetUnit = unitsInTargetHex[close];
            return targetUnit.Get(client.Data);
        }

        return null;
    }

    
    public Domain TopShowingDomain(HexGeneralData data)
    {
        for (var i = 0; i < _unitGraphics.DomainsByVisibilityPriority.Count; i++)
        {
            var domain = _unitGraphics.DomainsByVisibilityPriority[i];
            if (data.MapUnitHolder.HexUnitsByDomain[domain.MakeIdRef(data)]
                    .TryGetValue(_hex.MakeRef(), out var units)
                && units.Count > 0)
            {
                return domain;
            }
        }

        return null;
    }
}