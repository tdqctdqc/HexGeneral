using System.Collections.Generic;
using HexGeneral.Game;

namespace HexGeneral.Data.Components;

public interface ICombatInterruptorComponent
{
    IEnumerable<Hex> Radius(HexGeneralData data);
}