using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game.Client;

public partial class HexPanel : PanelContainer
{
    private HexMode _mode;
    private HexGeneralClient _client;
    public HexPanel(HexGeneralClient client, HexMode mode)
    {
        _client = client;
        _mode = mode;
        _mode.SelectedHex.SettingChanged.SubscribeForNode(v =>
        {
            Draw();
        }, this);
    }

    private void Draw()
    {
        this.ClearChildren();
        if (_mode.SelectedHex.Value is not Hex hex)
        {
            return;
        }

        var vbox = new VBoxContainer();
        vbox.CreateLabelAsChild("Hex: " + hex.Coords.ToString());
        if (hex.Regime.Fulfilled())
        {
            vbox.CreateLabelAsChild($"Regime: {hex.Regime.Get(_client.Data).RegimeModel.Get(_client.Data).Name}");
        }
        vbox.CreateLabelAsChild($"Landform: {hex.Landform.Get(_client.Data).Name}");
        vbox.CreateLabelAsChild($"Vegetation: {hex.Vegetation.Get(_client.Data).Name}");

        if (_client.Data.LocationHolder.Locations
            .TryGetValue(hex.MakeRef(), out var lRef))
        {
            var location = lRef.Get(_client.Data);
            foreach (var buildingRef in location.Buildings)
            {
                var building = buildingRef.Get(_client.Data);
                vbox.CreateLabelAsChild(building.Name);
            }
        }
        AddChild(vbox);
    }
}