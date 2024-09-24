using GodotUtilities.GameData;
using GodotUtilities.Server;
using HexGeneral.Game.Client;
using HexGeneral.Game.Logic;

namespace HexGeneral.Game;

public abstract class AttackType : Model
{
    public int NumAttacks { get; private set; }
    public abstract bool CanAttack(Unit unit, Hex target, HexGeneralData data);
    
}