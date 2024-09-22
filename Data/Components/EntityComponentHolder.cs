using System.Collections.Generic;
using System.Linq;
using GodotUtilities.Logic;

namespace HexGeneral.Game.Components;

public class EntityComponentHolder(List<IEntityComponent> components)
{
    public List<IEntityComponent> Components { get; private set; } = components;

    public void Add(IEntityComponent c, ProcedureKey key)
    {
        Components.Add(c);
        c.Added(key);
    }
    public void Remove(IEntityComponent c, ProcedureKey key)
    {
        Components.Remove(c);
        c.Removed(key);
    }

    public void TurnTick(ProcedureKey key)
    {
        foreach (var component in Components)
        {
            component.TurnTick(key);
        }
    }
    public T Get<T>() where T : IEntityComponent
    {
        return Components.OfType<T>().SingleOrDefault();
    }

    public bool Has<T>() where T : IEntityComponent
    {
        return Components.Any(c => c is T);
    }
}