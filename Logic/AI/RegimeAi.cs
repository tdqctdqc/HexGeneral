using GodotUtilities.GameData;
using GodotUtilities.Logic;

namespace HexGeneral.Game.Logic.AI;

public class RegimeAi
{
    public ERef<Regime> Regime { get; private set; }

    public RegimeAi(ERef<Regime> regime)
    {
        Regime = regime;
    }


    public void Calculate(LogicKey key)
    {
        
    }
}