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
    

    public void MarkActive(ProcedureKey key)
    {
        Active = true;
    }

    public void MarkInactive(ProcedureKey key)
    {
        Active = false;
    }

    public Control GetDisplay(HexGeneralClient client)
    {
        var vbox = new VBoxContainer();
        var unit = Unit.Get(client.Data);
        var model = mobilizer.Get(client.Data);
        var texture = new TextureRect();
        texture.Size = Vector2.One * 50f;
        texture.Texture = model.GetTexture();
        texture.StretchMode = TextureRect.StretchModeEnum.Keep;
        var moveRatio = unit.EntityComponents.Get<MoveCountComponent>().MovePointRatioRemaining;
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
                    }
                }, client);
                client.SubmitCommand(com);
            });
        activate.Disabled = CanActivate(client.Data) == false;

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
        remove.Disabled = CanActivate(client.Data) == false;
        
        return vbox;
    }

    public void TurnTick(ProcedureKey key)
    {
        if (Mobilizer.Get(key.Data).CanAttack == false)
        {
            MarkInactive(key);
        }
    }

    public void Added(ProcedureKey key)
    {
        var unit = Unit.Get(key.Data);
        if (CanAdd(unit, key.Data.Data()) == false)
        {
            throw new Exception();
        }
        Native = unit.EntityComponents.Get<INativeMoveComponent>();
        unit.EntityComponents.Remove(Native, key);
        key.Data.Data().Notices.UnitAltered?.Invoke(unit);
    }

    public void Removed(ProcedureKey key)
    {
        var unit = Unit.Get(key.Data);
        unit.EntityComponents.Add(Native, key);
        key.Data.Data().Notices.UnitAltered?.Invoke(unit);
    }

    public void Modify(CombatModifier modifier, 
        HexGeneralData data)
    {
        if (Active)
        {
            var damageTakenMult = mobilizer.Get(data).MoveType
                .GetDamageMult(modifier.Hex, data,
                    modifier.OnOffense);
            modifier.AddDamageTakenMult(damageTakenMult, "Mobilizer Terrain Effect");
        }
        else
        {
            Native.Modify(modifier, data);   
        }
    }

    public bool CanActivate(HexGeneralData data)
    {
        var unit = Unit.Get(data);
        return unit.EntityComponents.Get<MoveCountComponent>().CanMove() 
               && unit.Attacked == false;
    }

    public HashSet<Hex> GetMoveRadius(Unit unit, HexGeneralData data)
    {        
        var moveCount = unit.EntityComponents.Get<MoveCountComponent>();
        if (moveCount.CanMove() == false) return new HashSet<Hex>();
        var ratio = moveCount.MovePointRatioRemaining;
        var hex = unit.GetHex(data);
        var model = unit.UnitModel.Get(data);
        var regime = unit.Regime.Get(data);
        var selfRadius = Mobilizer.Get(data).MoveType
            .GetMoveRadius(hex, regime, model.MovePoints * ratio,
                data);
        if (Active)
        {
            return selfRadius;
        }
        var nativeRadius = Native.GetMoveRadius(unit, data);

        if(CanActivate(data))
        {
            return selfRadius.Union(nativeRadius).ToHashSet();
        }

        return nativeRadius;
    }

    public void DrawRadius(Unit unit, 
        MapOverlayDrawer mesh, HexGeneralData data)
    {
        var radius = GetMoveRadius(unit, data);
        var hexTris = HexExt.UnitHexTris;
        if (Active)
        {
            mesh.Draw(mb =>
            {
                foreach (var h in radius)
                {
                    mb.AddTris(hexTris
                            .Select(p => p + h.WorldPos()), 
                        Colors.White.Tint(.5f));
                }

            }, Vector2.Zero);
        }
        else
        {
            var nativeRadius = Native.GetMoveRadius(unit, data);
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
        var moveCount = unit.EntityComponents.Get<MoveCountComponent>();
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

    public static bool CanAdd(Unit u, HexGeneralData data)
    {
        return u.EntityComponents.Get<MoveCountComponent>().HasMoved() == false;
    }
}