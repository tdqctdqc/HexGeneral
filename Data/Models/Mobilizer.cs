using System.Collections.Generic;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class Mobilizer : Model
{
    public MoveType MoveType { get; private set; }
    public float DamageTakenMult { get; private set; }
    public float MovePoints { get; private set; }
    public float RecruitCost { get; private set; }
    public float IndustrialCost { get; private set; }
    public bool CanAttack { get; private set; }
    public HashSet<MoveType> AllowedNativeMoveTypes { get; private set; }
}