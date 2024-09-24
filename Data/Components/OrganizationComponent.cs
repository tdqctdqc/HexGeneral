using System.Linq;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using GodotUtilities.Ui;
using HexGeneral.Game.Client;
using HexGeneral.Game.Logic;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game.Components;

public class OrganizationComponent(ERef<Unit> unit, float organization) : IUnitCombatComponent
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public float Organization { get; private set; } = organization;

    public Control GetDisplay(GameClient client)
    {
        var vbox = new VBoxContainer();
        var data = client.Client().Data;
        var baseOrg = Unit.Get(data).UnitModel.Get(data)
            .Organization;
        vbox.CreateLabelAsChild($"Organization: {Organization} / {baseOrg}");
        
        return vbox;
    }

    public void TurnTick(ProcedureKey key)
    {
        var data = key.Data.Data();
        var holder = data.MapUnitHolder.UnitPositions;
        float regenRatio = .4f;
        var unit = Unit.Get(key.Data);
        if (holder.TryGetValue(Unit, out var hRef))
        {
            var hex = hRef.Get(data);
            var hostile = hex.GetNeighbors(data)
                .Where(n => n.Regime.Fulfilled() 
                            && n.Regime != unit.Regime);
            foreach (var h in hostile)
            {
                if (h.GetUnitRefs(data).Any())
                {
                    regenRatio = .15f;
                }
                else
                {
                    regenRatio = Mathf.Max(regenRatio, .25f);
                }
            }
        }
        
        IncrementOrganization(unit
            .UnitModel.Get(key.Data).Organization * regenRatio, key);

    }

    public void Added(ProcedureKey key)
    {
        
    }

    public void Removed(ProcedureKey key)
    {
        
    }
    
    private float GetEffect(HexGeneralData data)
    {
        return Organization / 
               Unit.Get(data).UnitModel.Get(data)
                   .Organization - 1f;
    }
    public void ModifyAsAttacker(CombatModifier modifier, HexGeneralData data)
    {
        modifier.DamageToDefender.AddMult(GetEffect(data), "Attacker organization");
    }

    public void ModifyAsDefender(CombatModifier modifier, HexGeneralData data)
    {
        modifier.DamageToAttacker.AddMult(GetEffect(data), "Defender organization");
    }

    public void AfterCombat(ProcedureKey key)
    {
        
    }

    public bool AttackBlocked(HexGeneralData data)
    {
        return Organization == 0f;
    }
    public void IncrementOrganization(float amount, ProcedureKey key)
    {
        Organization += amount;
        Organization = Mathf.Clamp(Organization, 0f, 
            Unit.Get(key.Data).UnitModel.Get(key.Data).Organization);
    }
}