using System.Collections.Generic;

namespace HexGeneral.Game.Components;

public interface IComponented 
{
    List<IComponent> Components { get; }
}