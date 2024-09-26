using System;
using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using GodotUtilities.Ui;
using HexGeneral.Data.Components;
using HexGeneral.Game.Client;
using HexGeneral.Game.Logic;
using HexGeneral.Logic.Procedures;

namespace HexGeneral.Game.Components;

public class CurrentAmmunitionComponent(ERef<Unit> unit, int currentAmmo, bool restocked) 
    : IUnitCombatComponent
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public int CurrentAmmo { get; private set; } = currentAmmo;
    public bool Restocked { get; private set; } = restocked;

    public Control GetDisplay(GameClient client)
    {
        
        var vbox = new VBoxContainer();
        drawInner(vbox);

        void drawInner(VBoxContainer vbox)
        {
            vbox.ClearChildren();
            var data = client.Client().Data;
            var ammo = Unit.Get(data).UnitModel.Get(data)
                .Components.Get<AmmunitionComponent>();

            var ammoCap = ammo.AmmoCap;
            var ammoCost = ammo.AmmoCost;

            if (ammoCap != 0)
            {
                vbox.CreateLabelAsChild($"Current Ammo: {CurrentAmmo} / {ammoCap}");
            }

            var unit = Unit.Get(data);
            var regime = unit.Regime.Get(data);
            var resupply = vbox.AddButton($"Resupply Ammo", () =>
            {
                var missingAmount = ammoCap - CurrentAmmo;

                if (missingAmount == 0) return;
                var hex = unit.GetHex(data);
                var supplyAvailability = SupplyLogic.GetSupplyAvailability(hex,
                    data);
                var industrialCap = Mathf.FloorToInt(regime.IndustrialPoints / ammoCost);
                var cap = Mathf.Min(missingAmount, industrialCap);
                cap = Mathf.Min(cap, Mathf.FloorToInt(ammoCap 
                                                      * supplyAvailability));
                if (cap == 0) return;
                var proc = new UnitRestockAmmoProcedure(unit.MakeRef(), cap);
                var inner = new DoProcedureCommand(proc);

                var com = CallbackCommand.Construct(inner,
                    () =>
                    {
                        drawInner(vbox);
                    }, client);
                client.SubmitCommand(com);
            });
            resupply.Disabled = CanRestock(data) == false;
        }
        
        return vbox;
    }


    public void TurnTick(ProcedureKey key)
    {
        Restocked = false;
    }

    public void Added(EntityComponentHolder holder, GodotUtilities.GameData.Data data)
    {
        
    }

    public void Removed(EntityComponentHolder holder, GodotUtilities.GameData.Data data)
    {
        
    }


    private float GetEffect(HexGeneralData data)
    {
        return Unit.Get(data).UnitModel.Get(data).Components
                   .Get<AmmunitionComponent>()
                   .AmmoCap > 0 
                         && CurrentAmmo == 0
            ? -.75f
            : 0f;
    }
    public void ModifyAsAttacker(CombatModifier modifier, HexGeneralData data)
    {
        modifier.DamageToDefender.AddMult(GetEffect(data), 
            "Attacker Ammo");
    }

    public void ModifyAsDefender(CombatModifier modifier, HexGeneralData data)
    {
        modifier.DamageToAttacker.AddMult(GetEffect(data), 
            "Defender Ammo");
    }

    public void AfterCombat(ProcedureKey key)
    {
        IncrementAmmo(-1, key);
    }

    public bool AttackBlocked(HexGeneralData data)
    {
        var ammoCap = Unit.Get(data).UnitModel.Get(data)
            .Components.Get<AmmunitionComponent>().AmmoCap;
        return ammoCap > 0 && CurrentAmmo == 0;
    }

    public bool DefendBlocked(HexGeneralData data)
    {
        return false;
    }

    public void IncrementAmmo(int amount, ProcedureKey key)
    {
        CurrentAmmo += amount;
        CurrentAmmo = Mathf.Clamp(CurrentAmmo, 0, 
            Unit.Get(key.Data).UnitModel.Get(key.Data)
                .Components.Get<AmmunitionComponent>().AmmoCap);
    }
    public bool CanRestock(HexGeneralData data)
    {
        return Unit.Get(data).Components.Get<MoveCountComponent>(data)
                   .HasMoved() == false;
    }
}