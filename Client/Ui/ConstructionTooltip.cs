using Godot;
using HexGeneral.Data.Components;
using HexGeneral.Game;
using HexGeneral.Game.Client;

namespace HexGeneral.Client.Ui;

public partial class ConstructionTooltip : Node2D
{
    public ConstructionTooltip(EngineerEntityComponent e, 
        ConstructableBuildingModel model,
        Hex hex,
        HexGeneralClient client)
    {
        var panel = new PanelContainer();
        AddChild(panel);
        var vbox = new VBoxContainer();
        panel.AddChild(vbox);
        vbox.CreateLabelAsChild(model.Name);
        var l1 = vbox.CreateLabelAsChild($"Engineer points: {e.CurrentEngineerPoints}/{model.EngineerPointCost}");
        l1.Modulate = e.CurrentEngineerPoints >= model.EngineerPointCost
            ? Colors.Green
            : Colors.Red;

        if (client.Data.EngineerProjects
            .GetBuildingProgress(model, hex, client.Data,
                out var progress))
        {
            vbox.CreateLabelAsChild($"Progress: {progress}/{model.EngineerPointCost}");
        }
        
        Scale = Vector2.One * .015f;
    }
    
    
    public ConstructionTooltip(EngineerEntityComponent e, 
        RoadModel model,
        Vector2I edge,
        HexGeneralClient client)
    {
        var panel = new PanelContainer();
        AddChild(panel);
        var vbox = new VBoxContainer();
        panel.AddChild(vbox);
        vbox.CreateLabelAsChild(model.Name);
        var l1 = vbox.CreateLabelAsChild($"Engineer points: {e.CurrentEngineerPoints}/{model.EngineerPointCost}");
        l1.Modulate = e.CurrentEngineerPoints >= model.EngineerPointCost
            ? Colors.Green
            : Colors.Red;

        if (client.Data.EngineerProjects
            .GetRoadProgress(model, edge, client.Data,
                out var progress))
        {
            vbox.CreateLabelAsChild($"Progress: {progress}/{model.EngineerPointCost}");
        }
        
        Scale = Vector2.One * .015f;
    }
}