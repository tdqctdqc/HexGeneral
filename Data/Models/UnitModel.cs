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
    public UnitModel()
    {
    }

    public Unit Instantiate(Regime regime, HexGeneralData data)
    {
        var u = new Unit(data.IdDispenser.TakeId(),
            regime.MakeRef(), HitPoints, Organization, AmmoCap,
            this.MakeIdRef(data));
        data.Entities.AddEntity(u, data);
        return u;
    }
}