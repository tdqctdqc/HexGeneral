using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;

namespace HexGeneral.Game.Logic.Editor;

public class ChangeRoadAction(Vector2I edge, ModelIdRef<RoadModel> newRoad, ModelIdRef<RoadModel> oldRoad)
    : EditorAction
{
    public Vector2I Edge { get; private set; } = edge;
    public ModelIdRef<RoadModel> NewRoad { get; private set; } = newRoad;
    public ModelIdRef<RoadModel> OldRoad { get; private set; } = oldRoad;

    public static ChangeRoadAction Construct(Vector2I edge, RoadModel newRoad, HexGeneralData data)
    {
        var oldRoad = data.RoadNetwork.Roads.TryGetValue(edge, out var old)
            ? old
            : new ModelIdRef<RoadModel>();
        return new ChangeRoadAction(edge, newRoad.MakeIdRef(data), oldRoad);
    }
    public override void Do(ProcedureKey key)
    {
        key.Data.Data().RoadNetwork.AddRoad(Edge, NewRoad.Get(key.Data), key);
    }

    public override EditorAction GetUndoAction()
    {
        return new ChangeRoadAction(Edge, OldRoad, NewRoad);
    }
}