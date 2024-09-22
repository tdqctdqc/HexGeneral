using System.Collections.Generic;
using System.Linq;
using Godot;
using HexGeneral.Game.Components;

namespace HexGeneral.Game.Logic;

public class CombatModifier(
    Unit unit,
    Unit target,
    bool onOffense,
    Hex hex)
{
    public Unit Unit { get; private set; } = unit;
    public Unit Target { get; private set; } = target;
    public Hex Hex { get; private set; } = hex;
    public bool OnOffense { get; private set; } = onOffense;
    public float SoftMultSum { get; private set; } = 0f;
    public float SoftConst { get; private set; } = 0f;
    public float HardMultSum { get; private set; } = 0f;
    public float HardConst { get; private set; } = 0f;
    public float DamageTakenMultSum { get; private set; } = 0f;
    public float DamageDealtMultSum { get; private set; } = 0f;
    public Dictionary<string, List<(string, float)>> Infos { get; private set; }
    public static CombatModifier Construct(Unit unit, Unit target, 
        bool onOffense, Hex hex, HexGeneralData data)
    {
        var m = new CombatModifier(unit, target, onOffense, hex);
        foreach (var c in unit.EntityComponents.Components.OfType<IUnitCombatComponent>())
        {
            c.Modify(m, data);
        }
        return m;
    }
    public static CombatModifier ConstructVerbose(Unit unit, Unit target, 
        bool onOffense, Hex hex, HexGeneralData data)
    {
        var m = new CombatModifier(unit, target, onOffense, hex);
        m.Infos = new Dictionary<string, List<(string, float)>>();
        foreach (var c in unit.EntityComponents.Components.OfType<IUnitCombatComponent>())
        {
            c.Modify(m, data);
        }
        return m;
    }

    public void AddDamageTakenMult(float value, string descr)
    {
        if (Infos is not null)
        {
            Infos.AddOrUpdate(
                    nameof(CombatModifier.DamageTakenMultSum),
                        (descr, value));
        }
        DamageTakenMultSum 
            += value;
    }
    
    public float GetDamageAgainst(
        CombatModifier targetModifier,
        HexGeneralData data)
    {
        var atkModel = Unit.UnitModel.Get(data);
        var defModel = Target.UnitModel.Get(data);
        var softMult = Mathf.Max(1f + targetModifier.SoftMultSum, 0f);
        var soft = (atkModel.SoftAttack + SoftConst)
                   * softMult
                   * (1f - defModel.Hardness);
        var hardMult = Mathf.Max(1f + targetModifier.HardMultSum, 0f);
        var hard = (atkModel.HardAttack + HardConst)
                   * hardMult
                   * defModel.Hardness;
        var damageDealtMult = Mathf.Max(0f, 1f + DamageDealtMultSum);
        var targetDamageTakenMult = Mathf.Max(0f, 1f + targetModifier.DamageTakenMultSum);
        
        return (soft + hard)
               * damageDealtMult
               * targetDamageTakenMult
               * CombatLogic.GetEffectiveAttackRatio(Unit, data);
    }

    public string Info()
    {
        var r = "";
        if (Infos is null) return r;
        foreach (var (field, mods) in Infos)
        {
            r += $"\n{field}: ";
            foreach (var (modName, modAmt) in mods)
            {
                r += $"\n.....{modName}: {modAmt}";
            }
        }

        return r;
    }
}