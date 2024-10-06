using System.Linq;
using Godot;
using GodotUtilities.GameClient;
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
            r => GetRegimeBorderColor(r, client), h => h.Regime.Get(client.Client().Data), client.Client());
        AddChild(RegimeBorder);
        RegimeBorder.ZIndex = (int)GraphicsLayers.RegimeBorders;
        RegimeBorder.ZAsRelative = false;
        MakeSettings(client);
    }

    public void Update(HexGeneralClient client)
    {
        RegimeBorder.Update(client);
        RegimeColor.SetColors(h => GetRegimeColor(h, client));
    }
    private Color GetRegimeColor(Hex hex, HexGeneralClient client)
    {
        var r = hex.Regime.Get(client.Data);
        if (r is null) return Colors.Transparent;
        return r.RegimeModel.Get(client.Client().Data).Color.Inverted();
    }
    private Color GetRegimeBorderColor(Regime r, HexGeneralClient client)
    {
        if (r is null) return Colors.Transparent;
        return r.RegimeModel.Get(client.Client().Data).Color.Inverted();
    }
    private void MakeSettings(HexGeneralClient client)
    {
        var colorTransparency = RegimeColor.MakeTransparencySetting("Regime Color Transparency");
        colorTransparency.Set(.2f);
        client.ClientSettings.SettingsOptions.Add(colorTransparency);
    }
    public void UpdateHex(Hex hex, HexGeneralClient client)
    {
        if (hex.Regime.IsEmpty()) return;
        var color = hex.Regime.Get(client.Data).RegimeModel
            .Get(client.Data).Color;
        RegimeColor.SetColor(hex, color);
        RegimeBorder.UpdateHex(hex, client);
    }
}