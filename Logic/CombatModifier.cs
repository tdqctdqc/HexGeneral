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
                .Components.OfType<IUnitCombatComponent>())
        {
            c.ModifyAsAttacker(this, data);
        }
        foreach (var c in Defender.Components
                .Components.OfType<IUnitCombatComponent>())
        {
            c.ModifyAsDefender(this, data);
        }
    }
}