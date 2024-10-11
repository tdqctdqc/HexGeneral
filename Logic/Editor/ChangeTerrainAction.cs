using GodotUtilities.GameData;
using GodotUtilities.Logic;
using MessagePack;

namespace HexGeneral.Game.Logic.Editor;

public class ChangeTerrainAction : EditorAction
{
    public static ChangeTerrainAction Construct(Hex hex, Landform newLf,
        HexGeneralData data)
    {
        var oldVeg = hex.Vegetation.Get(data);
        var oldLf = hex.Landform.Get(data);
        var newVeg = oldVeg.AllowedLandforms.Contains(newLf)
            ? oldVeg
            : data.ModelPredefs.Vegetations.Barren;
        return new ChangeTerrainAction(oldLf.MakeIdRef(data),
            newLf.MakeIdRef(data),
            oldVeg.MakeIdRef(data),
            newVeg.MakeIdRef(data), hex.MakeRef());
    }
    public static ChangeTerrainAction Construct(Hex hex, Vegetation newVeg,
        HexGeneralData data)
    {
        var oldVeg = hex.Vegetation.Get(data);
        var oldLf = hex.Landform.Get(data);
        newVeg = newVeg.AllowedLandforms.Contains(oldLf)
            ? newVeg
            : oldVeg;
        return new ChangeTerrainAction(oldLf.MakeIdRef(data),
            oldLf.MakeIdRef(data),
            oldVeg.MakeIdRef(data),
            newVeg.MakeIdRef(data), hex.MakeRef());
    }
    [SerializationConstructor] private ChangeTerrainAction(
        ModelIdRef<Landform> prevLandform, 
        ModelIdRef<Landform> newLandform, 
        ModelIdRef<Vegetation> prevVegetation, 
        ModelIdRef<Vegetation> newVegetation, 
        HexRef hex)
    {
        PrevLandform = prevLandform;
        NewLandform = newLandform;
        PrevVegetation = prevVegetation;
        NewVegetation = newVegetation;
        Hex = hex;
    }

    public HexRef Hex { get; private set; }
    public ModelIdRef<Landform> PrevLandform { get; private set; }
    public ModelIdRef<Landform> NewLandform { get; private set; }
    public ModelIdRef<Vegetation> PrevVegetation { get; private set; }
    public ModelIdRef<Vegetation> NewVegetation { get; private set; }
    public override void Do(ProcedureKey key)
    {
        var hex = Hex.Get(key.Data);
        hex.SetLandform(NewLandform.Get(key.Data), key);
        hex.SetVegetation(NewVegetation.Get(key.Data), key);
    }

    public override EditorAction GetUndoAction()
    {
        return new ChangeTerrainAction(NewLandform, PrevLandform,
            NewVegetation, PrevVegetation, Hex);
    }
}