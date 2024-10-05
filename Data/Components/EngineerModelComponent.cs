using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using HexGeneral.Game;

namespace HexGeneral.Data.Components;

public class EngineerModelComponent : IInheritedModelComponent
{
    public float EngineerPoints { get; private set; }
    public Control GetDisplay(GameClient client)
    {
        var vbox = new VBoxContainer();
        vbox.CreateLabelAsChild($"Engineer Points: {EngineerPoints}");
        return vbox;
    }

    public void InheritTo(IComponentedEntity entity, GodotUtilities.GameData.Data data)
    {
        var eComp = new EngineerEntityComponent(((Unit)entity).MakeRef(),
            EngineerPoints);
        entity.Components.Add(eComp, data);
    }
}