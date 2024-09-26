using GodotUtilities.GameData;
using HexGeneral.Game;
using HexGeneral.Game.Components;

namespace HexGeneral.Data.Components;

public class AmmunitionComponent : IInheritedModelComponent
{
    public int AmmoCap { get; private set; }
    public float AmmoCost { get; private set; }

    public void InheritTo(IComponentedEntity entity, 
        GodotUtilities.GameData.Data data)
    {
        var curr = new CurrentAmmunitionComponent(new ERef<Unit>(entity.Id),
            AmmoCap, false);
        entity.Components.Add(curr, data);
    }
}