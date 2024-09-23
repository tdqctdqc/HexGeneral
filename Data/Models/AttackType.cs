using GodotUtilities.GameData;
using GodotUtilities.Server;
using HexGeneral.Game.Client;

namespace HexGeneral.Game;

public abstract class AttackType : Model
{
    public int NumAttacks { get; private set; }
    public abstract bool CanAttack(Unit unit, Hex target, HexGeneralData data);
    public abstract void DrawRadius(Unit unit, MapOverlayDrawer overlay,
        HexGeneralClient client);
    public abstract void DrawPath(Unit unit, Unit target, 
        MapOverlayDrawer overlay, HexGeneralClient client);
    public abstract Command GetAttackCommand(Unit unit, Unit target, 
        HexGeneralClient client);
}