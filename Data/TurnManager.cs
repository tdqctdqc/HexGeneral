using System;
using System.Collections.Generic;
using System.Linq;
using GodotUtilities.GameData;
using GodotUtilities.Logic;

namespace HexGeneral.Game;

public class TurnManager(int id, int roundNumber, List<ERef<Regime>> regimeOrder, int regimeIter) : Entity(id)
{
    public int RoundNumber { get; private set; } = roundNumber;
    public int RegimeIter { get; private set; } = regimeIter;
    public List<ERef<Regime>> RegimeOrder { get; private set; } = regimeOrder;
    public bool AcceptingOrders { get; private set; }

    public ERef<Regime> GetCurrentRegime()
    {
        return RegimeOrder[RegimeIter];
    }
    public Player GetCurrentPlayer(HexGeneralData data)
    {
        var currRegime = GetCurrentRegime();
        return data.Entities.GetAll<Player>()
            .FirstOrDefault(p => p.Regime == currRegime);
    }
    public void Iterate(ProcedureKey key)
    {
        RegimeIter++;
        if (RegimeIter > RegimeOrder.Count - 1)
        {
            RegimeIter = 0;
            RoundNumber++;
        }
    }

    public void AcceptCommands(ProcedureKey key)
    {
        AcceptingOrders = true;
    }
    public void RejectCommands(ProcedureKey key)
    {
        AcceptingOrders = false;
    }
    public override void Made(GodotUtilities.GameData.Data d)
    {
        d.SetEntitySingleton<TurnManager>();
    }

    
    public override void CleanUp(GodotUtilities.GameData.Data d)
    {
        throw new Exception();
    }
}