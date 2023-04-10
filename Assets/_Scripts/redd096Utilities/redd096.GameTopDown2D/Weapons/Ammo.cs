using UnityEngine;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Weapons/Ammo")]
    public class Ammo : PickUpBASE<AdvancedWeaponComponent>
    {
        [Header("Ammo")]
        [SerializeField] string ammoType = "GunAmmo";
        [SerializeField] int quantity = 1;
        [Tooltip("Can pick when full of this type of ammo? If true, this object will be destroyed, but no ammo will be added")][SerializeField] bool canPickAlsoIfFull = false;

        public string AmmoType => ammoType;

        public override void PickUp()
        {
            //pick and add quantity
            whoHitComponent.AddAmmo(ammoType, quantity);
        }

        protected override bool CanPickUp()
        {
            //there is weapon component, and is not full of ammo or can pick anyway
            return whoHitComponent && (whoHitComponent.IsFullOfAmmo(ammoType) == false || canPickAlsoIfFull);
        }
    }
}