using System.Linq;
using Godot;
using GodotUtilities.Graphics;
using GodotUtilities.Ui;
using HexGeneral.Game.Client.Graphics;

namespace HexGeneral.Game.Client;

public partial class DeploymentPanel : PanelContainer
{
    private HexGeneralClient _client;
    private DeploymentMode _mode;
    public DeploymentPanel(DeploymentMode mode,
        HexGeneralClient client)
    {
        _mode = mode;
        _client = client;
        
        client.Data.Notices.UnitDeployed.Blank.Subscribe(Draw);
        Draw();
    }

    private void Draw()
    {
        
        this.ClearChildren();
        var clientPlayer = _client.GetPlayer();
        if (clientPlayer is null) return;

        var regime = clientPlayer.Regime.Get(_client.Data);
        
        var deployableUnits = regime.GetUnits(_client.Data)
            .Where(u => u.Deployed(_client.Data) == false).ToList();

        var unitScroll = new ItemListToken<Unit>(
            deployableUnits,
            m => m.UnitModel.Get(_client.Data).Name,
            m => m.UnitModel.Get(_client.Data).GetTexture(),
            50, false);
        unitScroll.JustSelected += () =>
        {
            var unit = unitScroll.Selected.Single();
            _mode.SelectedUnit.Set(unit);
        };
        AddChild(unitScroll.ItemList);
    }
}