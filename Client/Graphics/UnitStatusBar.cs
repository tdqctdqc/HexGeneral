using Godot;
using HexGeneral.Game.Components;

namespace HexGeneral.Game.Client.Graphics;

public partial class UnitStatusBar : Node2D
{
    public void Update(Unit unit, HexGeneralData data)
    {
        var model = unit.UnitModel.Get(data);
        var hitPointRatio = unit.CurrentHitPoints / model.HitPoints;
        var ammoRatio = (float)unit.CurrentAmmo / model.AmmoCap;
        var orgRatio = unit.CurrentOrganization / model.Organization;
    
        var healthIcon = GetNode<MeshInstance2D>("HealthIcon");
        healthIcon.Modulate = ColorsExt.GetHealthColor(hitPointRatio);
        
        var ammoIcon = GetNode<MeshInstance2D>("AmmoIcon");
        ammoIcon.Modulate = ColorsExt.GetHealthColor(ammoRatio);
        
        var moveIcon = GetNode<MeshInstance2D>("MoveIcon");
        moveIcon.Modulate = unit.Components.Get<MoveCountComponent>().CanMove()
            ? Colors.White : Colors.White.Tint(.25f);
        
        var attackIcon = GetNode<MeshInstance2D>("AttackIcon");
        attackIcon.Modulate = unit.Components.Get<AttackCountComponent>()
            .CanAttack(unit, data)
                ? Colors.White : Colors.White.Tint(.25f);
        
        var orgIcon = GetNode<MeshInstance2D>("OrgIcon");
        orgIcon.Modulate = ColorsExt.GetHealthColor(orgRatio);

    }
}