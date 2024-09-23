using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game.Components;

namespace HexGeneral.Logic.Procedures;

public class RemoveEntityComponentProcedure<TEntity, TComp>(ERef<TEntity> entity) : GodotUtilities.Server.Procedure
    where TEntity : Entity, IComponentedEntity
    where TComp : IEntityComponent
{
    public ERef<TEntity> Entity { get; private set; } = entity;

    public override void Handle(ProcedureKey key)
    {
        var e = Entity.Get(key.Data);
        var comp = e.Components.Get<TComp>();
        e.Components.Remove(comp, key);
    }
}