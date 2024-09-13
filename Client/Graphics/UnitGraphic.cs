using Godot;
using GodotUtilities.Graphics;

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
        _unitTexture.Texture = u.UnitModel.Get(data).GetTexture();
        AddChild(_unitTexture);

        _statusBar = SceneManager.Instance<UnitStatusBar>();
        _statusBar.Scale = Vector2.One / 7.5f;
        _statusBar.Position = Vector2.Down * _dim / 2f; 
        AddChild(_statusBar);
    }

    public void Update(Unit u, HexGeneralClient client)
    {
        
        // var mb = new MeshBuilder();
        _statusBar.Update(u, client.Data);
        // _health.Text = Mathf.FloorToInt(hitPointRatio * 100f).ToString();
        // var orgRatio = u.CurrentOrganization / model.Organization;
        // var ammoRatio = u.CurrentAmmo / model.AmmoCap;
        // var lineThickness = .025f;
        // var from = Vector2.Left * _dim / 4f;
        // var dir = Vector2.Right * _dim / 2f;
        // var down = Vector2.Down * lineThickness;
        // mb.AddLine(from + down * 0f, 
        //     from + dir * hitPointRatio + down * 0f,
        //     Colors.Red, lineThickness);
        // mb.AddLine(from + down * 1f, 
        //     from + dir * orgRatio + down * 1f,
        //     Colors.Yellow, lineThickness);
        // mb.AddLine(from + down * 2f, 
        //     from + dir * ammoRatio + down * 2f,
        //     Colors.Blue, lineThickness);
        // var bars = mb.GetMeshInstance();
        // bars.Position = Vector2.Down * _dim / 2f;
        // AddChild(bars);
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