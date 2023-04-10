using UnityEngine;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Pickables/Health Pick Up")]
    public class HealthPickUp : PickUpBASE<HealthComponent>
    {
        [Header("Health")]
        [SerializeField] float healthToGive = 30;
        [Tooltip("Can pick when full of health? If true, this object will be destroyed, but no health will be added")][SerializeField] bool canPickAlsoIfFull = false;

        public override void PickUp()
        {
            //give health
            whoHitComponent.GetHealth(healthToGive);
        }

        protected override bool CanPickUp()
        {
            //there is health component, and is not full of health or can pick anyway
            return whoHitComponent && (whoHitComponent.CurrentHealth < whoHitComponent.MaxHealth || canPickAlsoIfFull);
        }
    }
}