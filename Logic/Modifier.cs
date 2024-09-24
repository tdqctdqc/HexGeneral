using System.Collections.Generic;

namespace HexGeneral.Game.Logic;

public class Modifier
{
    public float Const { get; private set; }
    public float Mult { get; private set; }
    public List<string> Descrs { get; private set; }

    public Modifier(bool verbose)
    {
        Const = 0f;
        Mult = 0f;
        if (verbose) Descrs = new List<string>();
    }

    public float Modify(float source)
    {
        return (source + Const) * (1f + Mult);
    }

    public void AddConst(float k, string descr)
    {
        Const += k;
        if (Descrs is not null)
        {
            Descrs.Add($"{descr}: {k}");
        }
    }
    public void AddMult(float m, string descr)
    {
        Mult += m;
        if (Descrs is not null)
        {
            Descrs.Add($"{descr}: {m}");
        }
    }

    public string Print()
    {
        var res = "";
        foreach (var descr in Descrs)
        {
            res += $"\n{descr}";
        }
        return res;
    }
    
}