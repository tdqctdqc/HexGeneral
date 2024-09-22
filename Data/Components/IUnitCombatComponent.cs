using System.Collections.Generic;
using HexGeneral.Game.Logic;

namespace HexGeneral.Game.Components;

public interface IUnitCombatComponent : IEntityComponent
{
    void Modify(CombatModifier modifier, 
        HexGeneralData data);
}