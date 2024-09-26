using System.Collections.Generic;
using GodotUtilities.GameData;
using HexGeneral.Game.Components;

namespace HexGeneral.Game;

public class UnitModel : Model, IComponentedModel
{
    public float HitPoints { get; private set; }
    public float Organization { get; private set; }
    public float Hardness { get; private set; }
    public float RecruitCost { get; private set; }
    public float IndustrialCost { get; private set; }
    public float MovePoints { get; private set; }
    public MoveType MoveType { get; private set; }
    public ModelComponentHolder Components { get; private set; }
        = new ();
    
    public Unit Instantiate(Regime regime, HexGeneralData data)
    {
        var u = Unit.Instantiate(this, regime, data);
        return u;
    }

}