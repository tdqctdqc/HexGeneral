using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class UnitModel : Model
{
    public float HitPoints { get; private set; }
    public float Organization { get; private set; }
    public int AmmoCap { get; private set; }
    public int AmmoPerShot { get; private set; }
    public int Range { get; private set; }
    public float HardAttack { get; private set; }
    public float SoftAttack { get; private set; }
    public float Hardness { get; private set; }
    public float RecruitCost { get; private set; }
    public float IndustrialCost { get; private set; }
    public float MovePoints { get; private set; }
    public UnitModel(string name) : base(name)
    {
    }
}