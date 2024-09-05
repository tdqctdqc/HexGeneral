using Godot;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class Unit(int id, ERef<Regime> regime, float currentHitPoints, float currentOrganization, float currentAmmo, ModelIdRef<UnitModel> unitModel, float movePoints) : Entity(id)
{
    public ERef<Regime> Regime { get; private set; } = regime;
    public ModelIdRef<UnitModel> UnitModel { get; private set; } = unitModel;
    public float MovePoints { get; private set; } = movePoints;
    public float CurrentHitPoints { get; private set; } = currentHitPoints;
    public float CurrentOrganization { get; private set; } = currentOrganization;
    public float CurrentAmmo { get; private set; } = currentAmmo;
    
    public override void Made(Data d)
    {
        
    }

    public override void CleanUp(Data d)
    {
    }
}