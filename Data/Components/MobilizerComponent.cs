using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using GodotUtilities.Server;
using GodotUtilities.Ui;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Command;
using HexGeneral.Game.Logic;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game.Components;

public class MobilizerComponent
    (ModelIdRef<Mobilizer> mobilizer, bool active, ERef<Unit> unit, 
        INativeMoveComponent native) 
    : IMoveComponent
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public ModelIdRef<Mobilizer> Mobilizer { get; private set; } = mobilizer;
    public INativeMoveComponent Native { get; private set; } = native;
    public bool Active { get; private set; } = active;
    
    
    public void Activate(ProcedureKey key)
    {
        if (CanToggle(key.Data.Data()) == false) return;
        if (Active) return;
        var data = key.Data.Data();
        Active = true;
        key.Data.Data().Notices.UnitAltered?.Invoke(Unit.Get(data));
    }

    public void Deactivate(ProcedureKey key)
    {
        if (Active == false) return;
        if (CanToggle(key.Data.Data()) == false) return;
        Active = false;
        key.Data.Data().Notices.UnitAltered?
            .Invoke(Unit.Get(key.Data));
    }

    private void SwitchDomainIfNeeded(HexGeneralData data)
    {
        var (nativeDomain, mobDomain)
            = (Native
                    .GetActiveMoveType(data).Domain, 
                Mobilizer.Get(data).MoveType.Domain);
        if (nativeDomain == mobDomain) return;
        var (togglingToDomain, togglingFromDomain) = Active 
            ? (nativeDomain, mobDomain)
            : (mobDomain, nativeDomain);
        data.MapUnitHolder.SwitchUnitDomain(Unit.Get(data),
            togglingToDomain, data);
    }

    public Control GetDisplay(GameClient client)
    {
        var vbox = new VBoxContainer();
        var data = client.Client().Data;
        var unit = Unit.Get(data);
        var model = mobilizer.Get(data);
        var texture = new TextureRect();
        texture.Size = Vector2.One * 50f;
        texture.Texture = model.GetTexture();
        texture.StretchMode = TextureRect.StretchModeEnum.Keep;
        var moveRatio = unit.Components.Get<MoveCountComponent>(data).MovePointRatioRemaining;
        vbox.AddChild(texture);
        vbox.CreateLabelAsChild("Mobilizer: " + model.Name);
        vbox.CreateLabelAsChild($"Move Points: {model.MovePoints * moveRatio} / {model.MovePoints}");
        Button activate = null;
        activate = vbox.AddButton(Active ? "Deactivate" : "Activate",
            () =>
            {
                var newValue = Active == false;
                var proc = new SetUnitMobilizationProcedure(Unit, 
                    newValue);

                var inner = new DoProcedureCommand(proc);

                var com = CallbackCommand.Construct(inner, () =>
                {
                    if (GodotObject.IsInstanceValid(activate))
                    {
                        activate.Text = newValue ? "Deactivate" : "Activate";
                        activate.Disabled = CanToggle(data) == false;
                    }
                }, client);
                client.SubmitCommand(com);
            });
        activate.Disabled = CanToggle(data) == false;

        Button remove;
        remove = vbox.AddButton("Remove",
            () =>
            {
                var proc 
                    = new RemoveEntityComponentProcedure<Unit, MobilizerComponent>
                        (Unit);

                var inner = new DoProcedureCommand(proc);

                var com = CallbackCommand.Construct(inner, () =>
                {
                    if (GodotObject.IsInstanceValid(vbox))
                    {
                        vbox.ClearChildren();
                    }
                }, client);
                client.SubmitCommand(com);
            });
        remove.Disabled = CanToggle(data) == false;
        
        return vbox;
    }

    public void TurnTick(ProcedureKey key)
    {
        if (Mobilizer.Get(key.Data).CanAttack == false)
        {
            Deactivate(key);
        }
    }

    public void Added(EntityComponentHolder holder, GodotUtilities.GameData.Data data)
    {
        var unit = Unit.Get(data);
        if (CanAdd(unit, data.Data()) == false)
        {
            throw new Exception();
        }
        Native = unit.Components.Get<INativeMoveComponent>(data);
        holder.Remove(Native, data);
        data.Data().Notices.UnitAltered?.Invoke(unit);
    }

    public void Removed(EntityComponentHolder holder, GodotUtilities.GameData.Data data)
    {
        var unit = Unit.Get(data);
        unit.Components.Add(Native, data);
        data.Data().Notices.UnitAltered?.Invoke(unit);
    }
    
    public void ModifyAsAttacker(CombatModifier modifier, 
        HexGeneralData data)
    {
        if (Active)
        {
            Mobilizer.Get(data).ModifyAsAttacker(modifier, data);
        }
        else
        {
            Native.ModifyAsAttacker(modifier, data);   
        }
    }

    public void ModifyAsDefender(CombatModifier modifier, 
        HexGeneralData data)
    {
        if (Active)
        {
            Mobilizer.Get(data).ModifyAsDefender(modifier, data);
        }
        else
        {
            Native.ModifyAsDefender(modifier, data);   
        }
    }

    public void AfterCombat(ProcedureKey key)
    {
        
    }

    public bool CanToggle(HexGeneralData data)
    {
        var (nativeDomain, mobDomain)
            = (Native
                .GetActiveMoveType(data).Domain, 
                Mobilizer.Get(data).MoveType.Domain);
        var (togglingToDomain, togglingFromDomain) = Active 
            ? (nativeDomain, mobDomain)
            : (mobDomain, nativeDomain);
        var unit = Unit.Get(data);
        var hex = unit.GetHex(data);

        if (togglingFromDomain != togglingToDomain)
        {
            if (hex.Full(togglingToDomain, data)) return false;
        }
        
        return unit.Components.Get<MoveCountComponent>(data).CanMove() 
               && unit.Components.Get<AttackCountComponent>(data)
                    .AttacksTaken == 0;
    }

    public HashSet<Hex> GetMoveRadius(Unit unit, HexGeneralData data)
    {        
        var moveCount = unit.Components.Get<MoveCountComponent>(data);
        if (moveCount.CanMove() == false) return new HashSet<Hex>();
        var ratio = moveCount.MovePointRatioRemaining;
        var hex = unit.GetHex(data);
        var model = unit.UnitModel.Get(data);
        var regime = unit.Regime.Get(data);
        if (Active)
        {
            return Mobilizer.Get(data).MoveType
                .GetMoveRadius(hex, regime, model.MovePoints * ratio,
                    data);
        }
        return Native.GetMoveRadius(unit, data);
    }

    public void DrawRadius(Unit unit, 
        MapOverlayDrawer mesh, HexGeneralData data)
    {
        var radius = GetMoveRadius(unit, data);
        var nativeRadius = Native.GetMoveRadius(unit, data);

        var hexTris = HexExt.UnitHexTris;
        if (Active)
        {
            nativeRadius.ExceptWith(radius);
            mesh.Draw(mb =>
            {
                foreach (var h in radius)
                {
                    mb.AddTris(hexTris
                            .Select(p => p + h.WorldPos()), 
                        Colors.White.Tint(.5f));
                }

            }, Vector2.Zero);
            mesh.Draw(mb =>
            {
                foreach (var h in nativeRadius)
                {
                    mb.AddTris(hexTris
                            .Select(p => p + h.WorldPos()), 
                        Colors.Red.Tint(.5f));
                }

            }, Vector2.Zero);
        }
        else
        {
            radius.ExceptWith(nativeRadius);
            mesh.Draw(mb =>
            {
                foreach (var h in nativeRadius)
                {
                    mb.AddTris(hexTris
                            .Select(p => p + h.WorldPos()), 
                        Colors.White.Tint(.5f));
                }

            }, Vector2.Zero);
            mesh.Draw(mb =>
            {
                foreach (var h in radius)
                {
                    mb.AddTris(hexTris
                            .Select(p => p + h.WorldPos()), 
                        Colors.Red.Tint(.5f));
                }

            }, Vector2.Zero);
        }
    }

    public void DrawPath(Unit unit, Hex dest, 
        MapOverlayDrawer mesh, HexGeneralData data)
    {
        if (Active == false)
        {
            Native.DrawPath(unit, dest, mesh, data);
            return;
        }
        var path = Mobilizer.Get(data).MoveType
            .GetPath(unit.GetHex(data), dest, unit.Regime.Get(data),
                data, out var cost);
        if (path is null) return;
        mesh.Draw(mb =>
        {
            for (var i = 0; i < path.Count - 1; i++)
            {
                var from = path[i];
                var to = path[i + 1];
                mb.AddArrow(from.WorldPos(), to.WorldPos(), .25f, Colors.Black);
                mb.AddArrow(from.WorldPos(), to.WorldPos(), .2f, Colors.White);
            }
        }, Vector2.Zero);
        var tt = SceneManager.Instance<MoveTooltip>();

        tt.DrawInfo(unit, cost, data);
        mesh.AddNode(tt, dest.WorldPos());
    }

    public Command GetMoveCommand(Unit unit, Hex dest, 
        HexGeneralClient client)
    {
        var moveCount = unit.Components.Get<MoveCountComponent>(client.Data);
        if (moveCount.CanMove() == false) return null;
        if (Active == false)
        {
            return Native.GetMoveCommand(unit, dest, client);
        }

        var mob = Mobilizer.Get(client.Data);
        var path = mob.MoveType
            .GetPath(unit.GetHex(client.Data), dest, unit.Regime.Get(client.Data),
                client.Data, out var c);
        var costRatio = c / mob.MovePoints;

        if (path is null || costRatio > moveCount.MovePointRatioRemaining)
        {
            return null;
        }
        
        return new UnitMoveCommand(unit.MakeRef(), 
            path.Select(h => h.MakeRef()).ToList(), costRatio);
    }

    public float GetMovePoints(HexGeneralData data)
    {
        if (Active)
        {
            return Mobilizer.Get(data).MovePoints;
        }

        return Native.GetMovePoints(data);
    }

    public MoveType GetActiveMoveType(HexGeneralData data)
    {
        if (Active)
        {
            return Mobilizer.Get(data).MoveType;
        }

        return Native.GetActiveMoveType(data);
    }
    
    public bool AttackBlocked(HexGeneralData data)
    {
        return Active && Mobilizer.Get(data).CanAttack == false;
    }

    public bool DefendBlocked(HexGeneralData data)
    {
        return Active && Mobilizer.Get(data).CanDefend == false;
    }

    public static bool CanAdd(Unit u, HexGeneralData data)
    {
        return u.Components.Get<MoveCountComponent>(data).HasMoved() == false;
    }
}