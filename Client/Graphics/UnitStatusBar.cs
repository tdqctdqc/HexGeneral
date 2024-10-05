using System.Linq;
using Godot;
using HexGeneral.Data.Components;
using HexGeneral.Game.Components;

namespace HexGeneral.Game.Client.Graphics;

public partial class UnitStatusBar : Node2D
{
    public void Update(Unit unit, HexGeneralData data)
    {
        var model = unit.UnitModel.Get(data);
        var hitPointRatio = unit.CurrentHitPoints / model.HitPoints;
    
        var healthIcon = GetNode<MeshInstance2D>("HealthIcon");
        healthIcon.Modulate = ColorsExt.GetHealthColor(hitPointRatio);

        var org = unit.Components.Get<OrganizationComponent>(data);
        var orgRatio = org.Organization / model.Organization;
        var orgIcon = GetNode<MeshInstance2D>("OrgIcon");
        orgIcon.Modulate = ColorsExt.GetHealthColor(orgRatio);
        
        var ammoIcon = GetNode<MeshInstance2D>("AmmoIcon");
        if (unit.Components.Get<CurrentAmmunitionComponent>(data)
            is CurrentAmmunitionComponent ac)
        {
            var parent = model.Components.Get<AmmunitionComponent>();
            var ammoRatio = (float)ac.CurrentAmmo / parent.AmmoCap;
            ammoIcon.Modulate = ColorsExt.GetHealthColor(ammoRatio);
        }
        else
        {
            ammoIcon.Visible = false;
        }
        
        var moveIcon = GetNode<MeshInstance2D>("MoveIcon");
        moveIcon.Modulate = unit.Components.Get<MoveCountComponent>(data).CanMove()
            ? Colors.White : Colors.White.Tint(.25f);
        
        var attackIcon = GetNode<MeshInstance2D>("AttackIcon");
        var attackBlocked = unit.Components
            .OfType<IUnitCombatComponent>(data)
                .Any(c => c.AttackBlocked(data));
        
        attackIcon.Modulate = attackBlocked
            ? Colors.White.Tint(.25f) 
            : Colors.White;
        
        
    }
}