using Godot;
using GodotUtilities.GameClient;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game;

namespace HexGeneral.Data.Components;

public class EngineerEntityComponent(ERef<Unit> unit, float currentEngineerPoints) : IEntityComponent
{
    public ERef<Unit> Unit { get; private set; } = unit;
    public float CurrentEngineerPoints { get; private set; } = currentEngineerPoints;

    public Control GetDisplay(GameClient client)
    {
        var vbox = new VBoxContainer();
        var data = client.Client().Data;
        var unit = Unit.Get(data);
        var parent = unit.UnitModel.Get(data).Components.Get<EngineerModelComponent>();
        
        vbox.CreateLabelAsChild($"Engineer Points: {CurrentEngineerPoints} / {parent.EngineerPoints}");
        return vbox;
    }

    public void TurnTick(ProcedureKey key)
    {
        var data = key.Data.Data();
        var unit = Unit.Get(data);
        var parent = unit.UnitModel.Get(data).Components.Get<EngineerModelComponent>();
        CurrentEngineerPoints = parent.EngineerPoints;
    }

    public void SpendPoints(float spent, ProcedureKey key)
    {
        var data = key.Data.Data();
        var unit = Unit.Get(data);
        var parent = unit.UnitModel.Get(data).Components.Get<EngineerModelComponent>();
        CurrentEngineerPoints = Mathf.Clamp(CurrentEngineerPoints - spent, 0f, parent.EngineerPoints);
    }

    public void Added(EntityComponentHolder holder, GodotUtilities.GameData.Data data)
    {
        
    }

    public void Removed(EntityComponentHolder holder, GodotUtilities.GameData.Data data)
    {
        
    }
}