using System;
using Godot;
using GodotUtilities.DataStructures.Hex;
using GodotUtilities.GameData;
using GodotUtilities.Graphics;
using GodotUtilities.Server;
using HexGeneral.Game.Client;
using HexGeneral.Game.Client.Command;
using HexGeneral.Game.Client.Graphics;
using HexGeneral.Game.Components;
using HexGeneral.Game.Logic;

namespace HexGeneral.Game;

public class LandAttackType : AttackType
{
    public override bool CanAttack(Unit unit, Hex target, 
        HexGeneralData data)
    {
        var model = unit.UnitModel.Get(data);
        return unit.GetHex(data).Coords.GetHexDistance(target.Coords) <= model.Range;
    }
}