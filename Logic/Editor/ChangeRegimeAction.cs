using GodotUtilities.GameData;
using GodotUtilities.Logic;
using MessagePack;

namespace HexGeneral.Game.Logic.Editor;

public class ChangeRegimeAction : EditorAction
{
    public static ChangeRegimeAction Construct(Hex hex, Regime newRegime,
        HexGeneralData data)
    {
        return new ChangeRegimeAction(hex.MakeRef(),
            hex.Landform.Get(data).IsLand
                ? newRegime.MakeRef()
                : new ERef<Regime>(),
            hex.Regime);
    }
    [SerializationConstructor] private ChangeRegimeAction(HexRef hex, 
        ERef<Regime> newRegime, ERef<Regime> oldRegime)
    {
        Hex = hex;
        NewRegime = newRegime;
        OldRegime = oldRegime;
    }

    public HexRef Hex { get; private set; }
    public ERef<Regime> NewRegime { get; private set; }
    public ERef<Regime> OldRegime { get; private set; }
    public override void Do(ProcedureKey key)
    {
        Hex.Get(key.Data).SetRegime(NewRegime, key);
    }

    public override EditorAction GetUndoAction()
    {
        return new ChangeRegimeAction(Hex, OldRegime, NewRegime);
    }
}