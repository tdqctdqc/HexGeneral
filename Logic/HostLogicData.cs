using System.Collections.Generic;
using System.Threading.Tasks;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using HexGeneral.Game.Logic.AI;

namespace HexGeneral.Game.Logic;

public class HostLogicData
{
    public HostLogicData(Dictionary<ERef<Regime>, RegimeAi> ais)
    {
        Ais = ais;
    }

    public Dictionary<ERef<Regime>, RegimeAi> Ais { get; private set; }

    public async void HandleAi(LogicKey key)
    {
        var res = Task.Run(() => DoAiIfNoPlayer(key));
        await res;
    }
    
    
    private void DoAiIfNoPlayer(LogicKey key)
    {
        var data = key.Data.Data();
        var currRegime = data.TurnManager.GetCurrentRegime();
        if (data.TurnManager.GetCurrentPlayer(data) is not null)
        {
            return;
        }
        
        if (Ais.TryGetValue(currRegime, out var ai) == false)
        {
            ai = new RegimeAi(currRegime);
            Ais.Add(currRegime, ai);
        }
        
        ai.Calculate(key);
    }
}