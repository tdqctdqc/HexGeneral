using System;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using GodotUtilities.Server;
using HexGeneral.Game;
using HexGeneral.Game.Client;
using HexGeneral.Game.Components;
using HexGeneral.Game.Logic;

namespace HexGeneral.Data.Components;

public class SurfaceToAirAttackComponent : AttackComponent
{
    public float AntiAirAttack { get; private set; }
    public int Range { get; private set; }
    public override bool CanAttack(Unit targetUnit, Hex targetHex, HexGeneralData data)
    {
        var targetMoveType = targetUnit.Components.Get<IMoveComponent>(data)
            .GetActiveMoveType(data);
        if (targetMoveType.Domain != data.ModelPredefs.Domains.AirDomain)
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
        vbox.CreateLabelAsChild("Anti Air Attack: " + AntiAirAttack);
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

    public override void ModifyAsAttacker(CombatModifier modifier, HexGeneralData data)
    {
        // if (CanAttack(modifier.Defender, modifier.Hex, data) == false)
        // {
        //     return;
        // }
        // modifier.DamageToDefender.AddConst(AntiAirAttack, "Anti Air Attack");
    }

    public override void ModifyAsDefender(CombatModifier modifier, HexGeneralData data)
    {
        if (CanAttack(modifier.Attacker, modifier.Hex, data) == false)
        {
            return;
        }
        modifier.DamageToAttacker.AddConst(AntiAirAttack, "Anti Air Attack");
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