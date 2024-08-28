using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GodotUtilities.GameData;
using GodotUtilities.Reflection;

namespace HexGeneral.Game;

public interface IPredefHolder<T>
{
    
}

public static class IPredefHolderExt
{
    public static IEnumerable<T> GetPredefs<T>(this IPredefHolder<T> holder)
    {
        return holder.GetType().GetPropertiesOfType<T>();
    }
    
    public static Dictionary<string, T> GetPredefsByName<T>
        (this IPredefHolder<T> holder) where T : Model
    {
        return holder.GetType().GetPropertiesOfType<T>()
            .ToDictionary(t => t.Name, t => t);
    }
}