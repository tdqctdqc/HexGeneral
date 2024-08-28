using System.Collections.Generic;
using GodotUtilities.DataStructures;
using GodotUtilities.GameData;
using GodotUtilities.Serialization;

namespace HexGeneral.Game;

public class HexGeneralData : Data
{
    public ModelPredefs ModelPredefs { get; private set; }
    public HexGeneralData() 
        : base(new IdDispenser(0), 
            new Entities(new Dictionary<int, Entity>()),
            new Models(),
            new Serializer())
    {
        ModelPredefs = new ModelPredefs();
    }
}