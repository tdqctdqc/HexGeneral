using System.Collections.Generic;
using GodotUtilities.Logic;
using HexGeneral.Game.Logic;

namespace HexGeneral.Game.Components;

public interface IUnitCombatComponent : IEntityComponent
{
    void ModifyAsAttacker(CombatModifier modifier, 
        HexGeneralData data);
    void ModifyAsDefender(CombatModifier modifier, 
        HexGeneralData data);

    void AfterCombat(ProcedureKey key);
    bool AttackBlocked(HexGeneralData data);
}