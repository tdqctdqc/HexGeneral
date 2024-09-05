using Godot;
using GodotUtilities.Graphics;

namespace HexGeneral.Game.Client.Graphics;

public partial class UnitGraphic : Node2D
{
    private static Mesh _mesh;
    private MeshInstance2D _unitTexture;
    private MeshInstance2D _bars;
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
        
        
        
        
        if (_mesh is null)
        {
            var q = new QuadMesh();
            _mesh = q;
            q.Size = Vector2.One * _dim;
        }
        _unitTexture = new MeshInstance2D();
        _unitTexture.Scale = new Vector2(1f, -1f);
        _unitTexture.Mesh = _mesh;
        _unitTexture.Texture = u.UnitModel.Get(data).GetTexture();
        AddChild(_unitTexture);
    }

    public void Update(Unit u, HexGeneralData data)
    {
        _bars?.Free();
        var mb = new MeshBuilder();
        var model = u.UnitModel.Get(data);
        var hitPointRatio = u.CurrentHitPoints / model.HitPoints;
        var orgRatio = u.CurrentOrganization / model.Organization;
        var ammoRatio = u.CurrentAmmo / model.AmmoCap;
        var lineThickness = .025f;
        var from = Vector2.Left * _dim / 4f;
        var dir = Vector2.Right * _dim / 2f;
        var down = Vector2.Down * lineThickness;
        mb.AddLine(from + down * 0f, 
            from + dir * hitPointRatio + down * 0f,
            Colors.Red, lineThickness);
        mb.AddLine(from + down * 1f, 
            from + dir * orgRatio + down * 1f,
            Colors.Yellow, lineThickness);
        mb.AddLine(from + down * 2f, 
            from + dir * ammoRatio + down * 2f,
            Colors.Blue, lineThickness);
        var bars = mb.GetMeshInstance();
        bars.Position = Vector2.Down * _dim / 2f;
        AddChild(bars);
    }

    public void SetHighlight(bool highlight)
    {
        _highlight.Visible = highlight;
    }
}