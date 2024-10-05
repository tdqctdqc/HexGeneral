using System.Linq;
using Godot;

namespace HexGeneral.Game.Client.Graphics;

public partial class ConstructionGraphics : MultiMeshInstance2D
{
    public ConstructionGraphics(HexGeneralClient client)
    {
        var mesh = new QuadMesh();
        mesh.Size = Vector2.One;
        Multimesh = new MultiMesh();
        Multimesh.Mesh = mesh;
        Texture = TextureManager.GetTexture("Construction");
        DrawConstructions(client);
    }

    public void DrawConstructions(HexGeneralClient client)
    {
        var projs = client.Data.EngineerProjects;
        var hexes = projs
            .BuildingConstructionProgresses.Keys
            .Select(h => h.Get(client.Data))
            .ToHashSet();
        var map = client.Data.Map;
        hexes.UnionWith(projs.RoadConstructionProgresses
            .Keys.Select(k => map.Hexes[map.CoordsById[k.X]]));
        hexes.UnionWith(projs.RoadConstructionProgresses
            .Keys.Select(k => map.Hexes[map.CoordsById[k.Y]]));
        Multimesh.InstanceCount = hexes.Count;
        int iter = 0;
        foreach (var hex in hexes)
        {
            Multimesh.SetInstanceTransform2D(iter, new Transform2D(0f, 
                new Vector2(1f, -1f), 0f, hex.WorldPos()));
            iter++;
        }
    }
}