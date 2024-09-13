using System;
using GodotUtilities.GameData;
using GodotUtilities.Logic;

namespace HexGeneral.Game;

public class Player(int id, ERef<Regime> regime, Guid guid) : Entity(id)
{
    public ERef<Regime> Regime { get; private set; } = regime;
    public Guid Guid { get; private set; } = guid;

    public void SetRegime(ERef<Regime> regime, ProcedureKey key)
    {
        Regime = regime;
    }
    
    public override void Made(Data d)
    {
        ((HexGeneralData)d).PlayerHolder.PlayerByGuid.Add(Guid, this.MakeRef());
    }

    public override void CleanUp(Data d)
    {
        ((HexGeneralData)d).PlayerHolder.PlayerByGuid.Remove(Guid);
    }
}