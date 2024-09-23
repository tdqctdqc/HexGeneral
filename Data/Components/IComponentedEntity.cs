using System.Collections.Generic;

namespace HexGeneral.Game.Components;

public interface IComponentedEntity 
{
    EntityComponentHolder Components { get; }
}