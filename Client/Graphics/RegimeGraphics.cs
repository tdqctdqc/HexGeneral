using System.Linq;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;

namespace HexGeneral.Game.Client.Graphics;

public partial class RegimeGraphics : Node2D
{
    public HexColorMultiMesh<Hex> RegimeColor { get; private set; }
    public HexBorderMultiMesh<Regime> RegimeBorder { get; private set; }

    public RegimeGraphics(HexGeneralClient client)
    {
        RegimeColor = new HexColorMultiMesh<Hex>(
            client.Client().Data.Map.Hexes.Values.ToList(),
            h => h.WorldPos(),
            h => h.Regime.Fulfilled()
                ? h.Regime.Get(client.Data).RegimeModel.Get(client.Data).Color
                : Colors.Transparent, 1f);
        AddChild(RegimeColor);
        RegimeColor.ZIndex = (int)GraphicsLayers.RegimeFill;
        RegimeColor.ZAsRelative = false;

        RegimeBorder = new HexBorderMultiMesh<Regime>(
            r =>
            {
                if (r is null) return Colors.Transparent;
                return r.RegimeModel.Get(client.Client().Data).Color.Inverted();
            }, h => h.Regime.Get(client.Client().Data), client.Client());
        AddChild(RegimeBorder);
        RegimeBorder.ZIndex = (int)GraphicsLayers.RegimeBorders;
        RegimeBorder.ZAsRelative = false;
        MakeSettings(client);
    }

    private void MakeSettings(HexGeneralClient client)
    {
        var colorTransparency = RegimeColor.MakeTransparencySetting("Regime Color Transparency");
        colorTransparency.Set(.2f);
        client.ClientSettings.SettingsOptions.Add(colorTransparency);
    }
    public void UpdateHex(Hex hex, HexGeneralClient client)
    {
        var color = hex.Regime.Get(client.Data).RegimeModel.Get(client.Data).Color;
        RegimeColor.SetColor(hex, color);
        RegimeBorder.UpdateHex(hex, client);
    }
}