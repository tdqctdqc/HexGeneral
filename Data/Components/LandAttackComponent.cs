using System;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using GodotUtilities.Logic;
using GodotUtilities.Server;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Command;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Game.Components;
using HexGeneral.Game.Logic;

namespace HexGeneral.Game;

public class LandAttackComponent : AttackComponent
{
    public float HardAttack { get; private set; }
    public float SoftAttack { get; private set; }
    public int Range { get; private set; }
    
    public override bool CanAttack(Unit targetUnit, Hex targetHex, 
        HexGeneralData data)
    {
        if (targetHex.Landform.Get(data).IsLand == false) return false;
        var targetMoveType = targetUnit.Components.Get<IMoveComponent>(data)
            .GetActiveMoveType(data);
        if (targetMoveType.Domain != data.ModelPredefs.Domains.LandDomain
            && targetMoveType.Domain != data.ModelPredefs.Domains.AirDomain)
        {
            return false;
        }
        return targetUnit.GetHex(data).Coords
            .GetHexDistance(targetHex.Coords) 
               <= Range;
    }

    public override Control GetDisplay(GameClient client)
    {
        var vbox = new VBoxContainer();
        vbox.CreateLabelAsChild("Land Attack");
        vbox.CreateLabelAsChild($"Soft Attack: {SoftAttack}");
        vbox.CreateLabelAsChild($"Hard Attack: {HardAttack}");
        vbox.CreateLabelAsChild($"Range: {Range}");
        return vbox;
    }

    public override void TurnTick(ProcedureKey key)
    {
    }

    public override void Added(EntityComponentHolder holder, GodotUtilities.GameData.Data data)
    {
        
    }

    public override void Removed(EntityComponentHolder holder, GodotUtilities.GameData.Data data)
    {
        
    }


    public override void ModifyAsAttacker(CombatModifier modifier, 
        HexGeneralData data)
    {
        if (CanAttack(modifier.Defender, modifier.Hex, data) == false)
        {
            return;
        }

        var target = modifier.Defender;
        var targetModel = target.UnitModel.Get(data);
        var targetMoveType = target.Components.Get<IMoveComponent>(data)
            .GetActiveMoveType(data);
        
        if (targetMoveType.Domain == data.ModelPredefs.Domains.LandDomain
            || targetMoveType.Domain == data.ModelPredefs.Domains.AirDomain)
        {
            var hardness = targetModel.Hardness;
            var raw = SoftAttack * (1f - hardness)
                      + HardAttack * hardness;
            modifier.DamageToDefender.AddConst(raw, "Land Attack");
        }
    }

    public override void ModifyAsDefender(CombatModifier modifier, HexGeneralData data)
    {
        if (CanAttack(modifier.Attacker, modifier.Hex, data) == false)
        {
            return;
        }

        var target = modifier.Attacker;
        var targetModel = target.UnitModel.Get(data);
        var targetMoveType = target.Components.Get<IMoveComponent>(data)
            .GetActiveMoveType(data);
        if (targetMoveType.Domain == data.ModelPredefs.Domains.LandDomain)
        {
            var hardness = targetModel.Hardness;
            var raw = SoftAttack * (1f - hardness)
                      + HardAttack * hardness;
            modifier.DamageToAttacker.AddConst(raw, "Land Attack");
        }
    }

    public override void AfterCombat(ProcedureKey key)
    {
    }

    public override bool AttackBlocked(HexGeneralData data)
    {
        return false;
    }

    public override bool DefendBlocked(HexGeneralData data)
    {
        return false;
    }

    
}