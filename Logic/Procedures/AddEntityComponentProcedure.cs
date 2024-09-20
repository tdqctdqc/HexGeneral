using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game;
using HexGeneral.Game.Components;

namespace HexGeneral.Logic.Procedures;

public class AddEntityComponentProcedure<T>(ERef<T> entity, IComponent component) : GodotUtilities.Server.Procedure
    where T : Entity, IComponented
{
    public ERef<T> Entity { get; private set; } = entity;
    public IComponent Component { get; private set; } = component;

    public override void Handle(ProcedureKey key)
    {
        Entity.Get(key.Data).Components.Add(Component);
        Component.Added(key);
    }
}