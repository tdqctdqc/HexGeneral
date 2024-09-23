using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game;
using HexGeneral.Game.Components;

namespace HexGeneral.Logic.Procedures;

public class AddEntityComponentProcedure<T>(ERef<T> entity, IEntityComponent entityComponent) : GodotUtilities.Server.Procedure
    where T : Entity, IComponentedEntity
{
    public ERef<T> Entity { get; private set; } = entity;
    public IEntityComponent EntityComponent { get; private set; } = entityComponent;

    public override void Handle(ProcedureKey key)
    {
        Entity.Get(key.Data).Components.Add(EntityComponent, key);
    }
}