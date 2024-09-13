using Godot;
using GodotUtilities.Ui;

namespace HexGeneral.Game.Client;

public partial class UnitPanel : PanelContainer
{
    private UnitMode _mode;
    private HexGeneralClient _client;
    public UnitPanel(UnitMode mode, HexGeneralClient client)
    {
        _client = client;
        _mode = mode;
        _mode.SelectedUnit.SettingChanged.SubscribeForNode(v =>
        {
            Draw();
        }, this);
        TreeEntered += Draw;
        VisibilityChanged += () =>
        {
            if (Visible) Draw();
        };
        
    }

    private void Draw()
    {
        this.ClearChildren();
        var vbox = new VBoxContainer();
        AddChild(vbox);
        var unit = _mode.SelectedUnit.Value;
        if (unit is null) return;
        var model = unit.UnitModel.Get(_client.Data);
        var texture = new TextureRect();
        texture.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
        texture.Texture = model.GetTexture();
        texture.Size = Vector2.One * 20f;
        texture.CustomMinimumSize = texture.Size;
        vbox.AddChild(texture);
        vbox.CreateLabelAsChild(model.Name);
        vbox.CreateLabelAsChild($"Hitpoints: {unit.CurrentHitPoints}/{model.HitPoints}");
        vbox.CreateLabelAsChild($"Organization: {unit.CurrentOrganization}/{model.Organization}");
        vbox.CreateLabelAsChild($"Ammunition: {unit.CurrentAmmo}/{model.AmmoCap}");
        vbox.CreateLabelAsChild($"Move Points: {unit.MovePoints}/{model.MovePoints}");
        vbox.AddChild(new VSeparator());
        vbox.CreateLabelAsChild($"Soft Attack: {model.SoftAttack}");
        vbox.CreateLabelAsChild($"Hard Attack: {model.HardAttack}");
        vbox.CreateLabelAsChild($"Hardness: {model.Hardness}");
        vbox.CreateLabelAsChild($"Range: {model.Range}");


        vbox.AddButton($"Reinforce", () =>
        {
            
        });
        
        vbox.AddButton($"Resupply Ammo", () =>
        {
            
        });
        
    }
}