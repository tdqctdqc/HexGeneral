using System;
using Godot;

namespace HexGeneral.Game.Client.Graphics;

public partial class HexLocationGraphic : MeshInstance2D
{
    public HexLocationGraphic()
    {
        var mesh = new QuadMesh();
        mesh.Size = Vector2.One;
        Mesh = mesh;
        Scale = new Vector2(1f, -1f);
    }

    public void DrawLocation(Location location, HexGeneralClient client)
    {
        this.ClearChildren();
        Texture2D icon = null;
        var airbase = client.Data.ModelPredefs.Buildings.Airbase;
        var port = client.Data.ModelPredefs.Buildings.Port;
        foreach (var modelIdRef in location.Buildings)
        {
            var bModel = modelIdRef.Get(client.Data);
            if (bModel == airbase)
            {
                if (icon is not null) throw new Exception();
                icon = airbase.GetTexture();
                break;
            }
            if (bModel == port)
            {
                if (icon is not null) throw new Exception();
                icon = port.GetTexture();
                break;
            }
        }

        if (icon is not null)
        {
            Texture = icon;
            Visible = true;
        }
        else
        {
            Texture = null;
            Visible = false;
        }
    }
}