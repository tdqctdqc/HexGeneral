using System.Collections.Generic;
using System.Linq;
using Godot;
using HexGeneral.Game.Components;

namespace HexGeneral.Game.Logic;

public class CombatModifier
{
    public Modifier DamageToDefender { get; private set; }
    public Modifier DamageToAttacker { get; private set; }
    public Unit Attacker { get; private set; }
    public Unit Defender { get; private set; }
    public Hex Hex { get; private set; }
    public CombatModifier(Unit attacker,
        Unit defender,
        Hex hex,
        bool verbose,
        HexGeneralData data)
    {
        Attacker = attacker;
        Defender = defender;
        DamageToAttacker = new Modifier(verbose);
        DamageToDefender = new Modifier(verbose);
        Hex = hex;

        foreach (var c in Attacker.Components
                     .OfType<IUnitCombatComponent>(data))
        {
            c.ModifyAsAttacker(this, data);
        }

        var defBlocked = false;
        foreach (var c in Defender.Components
                .OfType<IUnitCombatComponent>(data))
        {
            c.ModifyAsDefender(this, data);
            defBlocked = defBlocked || c.DefendBlocked(data);
        }

        if (defBlocked)
        {
            DamageToAttacker.AddConst(-DamageToAttacker.Const,
                "defense blocked");
        }
    }
}