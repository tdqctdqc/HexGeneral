using System;
using System.Collections.Generic;
using GodotUtilities.DataStructures;
using GodotUtilities.GameData;

namespace HexGeneral.Game;

public class CombatNet(int id, TwoWay<ERef<Unit>, HexRef> net) : Entity(id)
{
    public TwoWay<ERef<Unit>, HexRef> Net { get; private set; } = net;
    
    public override void Made(GodotUtilities.GameData.Data d)
    {
        
    }

    public override void CleanUp(GodotUtilities.GameData.Data d)
    {
        throw new Exception();
    }
}