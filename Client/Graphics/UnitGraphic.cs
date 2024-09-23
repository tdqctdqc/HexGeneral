using System.Linq;
using Godot;
using GodotUtilities.Graphics;
using HexGeneral.Game.Components;

namespace HexGeneral.Game.Client.Graphics;

public partial class UnitGraphic : Node2D
{
    private static LabelSettings _labelSettings = GetLabelSettings();
    private static Mesh _unitMesh, _iconMesh;
    private MeshInstance2D _unitTexture;
    private UnitStatusBar _statusBar;
    private MeshInstance2D _highlight;
    private static float _dim = 1f;
    public UnitGraphic(Unit u, HexGeneralData data)
    {
        var mb = new MeshBuilder();
        mb.DrawBox((Vector2.Left + Vector2.Up) * _dim / 2f,
            (Vector2.Right + Vector2.Down) * _dim / 2f,
            Colors.White, .1f);
        _highlight = mb.GetMeshInstance();
        AddChild(_highlight);
        _highlight.Visible = false;
        
        if (_unitMesh is null)
        {
            var q = new QuadMesh();
            _unitMesh = q;
            q.Size = Vector2.One * _dim;

            var q2 = new QuadMesh();
            _iconMesh = q2;
            q2.Size = Vector2.One * _dim / 5f;
        }
        _unitTexture = new MeshInstance2D();
        _unitTexture.Scale = new Vector2(1f, -1f);
        _unitTexture.Mesh = _unitMesh;


        if (u.Components.Get<MobilizerComponent>()
            is { Active: true } m)
        {
            _unitTexture.Texture = m.Mobilizer.Get(data).GetTexture();
        }
        else
        {
            _unitTexture.Texture = u.UnitModel.Get(data).GetTexture();
        }
        AddChild(_unitTexture);

        _statusBar = SceneManager.Instance<UnitStatusBar>();
        _statusBar.Scale = Vector2.One / 7.5f;
        _statusBar.Position = Vector2.Down * _dim / 2f; 
        AddChild(_statusBar);
    }

    public void Update(Unit u, HexGeneralClient client)
    {
        if (u.Components.Get<MobilizerComponent>()
            is { Active: true } m)
        {
            _unitTexture.Texture = m.Mobilizer.Get(client.Data).GetTexture();
        }
        else
        {
            _unitTexture.Texture = u.UnitModel.Get(client.Data).GetTexture();
        }
        _statusBar.Update(u, client.Data);
    }

    public void SetHighlight(bool highlight)
    {
        _highlight.Visible = highlight;
    }

    private static LabelSettings GetLabelSettings()
    {
        var ls = new LabelSettings();
        ls.FontSize = 64;
        return ls;
    }
}