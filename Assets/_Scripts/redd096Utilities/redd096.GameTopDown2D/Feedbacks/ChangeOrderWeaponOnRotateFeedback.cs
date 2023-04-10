using UnityEngine;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/Change Order Weapon On Rotate Feedback")]
    public class ChangeOrderWeaponOnRotateFeedback : FeedbackRedd096<WeaponBASE>
    {
        [Header("Sprites - default get in children")]
        [SerializeField] SpriteRenderer[] spritesToUse = default;
        [SerializeField] int layerWhenLookingRight = 1;
        [SerializeField] int layerWhenLookingLeft = -1;

        AimComponent aimComponent;

        protected override void OnEnable()
        {
            //get referemces
            if (spritesToUse == null || spritesToUse.Length <= 0) spritesToUse = GetComponentsInChildren<SpriteRenderer>();

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            //remove owner events too
            OnDropWeapon();
        }

        protected override void AddEvents()
        {
            base.AddEvents();

            //add events
            owner.onPickWeapon += OnPickWeapon;
            owner.onDropWeapon += OnDropWeapon;

            //if there is an owner register to its events too
            OnPickWeapon();
        }

        protected override void RemoveEvents()
        {
            base.RemoveEvents();

            //remove events
            owner.onPickWeapon -= OnPickWeapon;
            owner.onDropWeapon -= OnDropWeapon;
        }

        void UpdateOrderInLayer(bool isLookingRight)
        {
            //foreach sprite, update order
            foreach (SpriteRenderer sprite in spritesToUse)
            {
                sprite.sortingOrder = isLookingRight ? layerWhenLookingRight : layerWhenLookingLeft;
            }
        }

        #region events

        void OnPickWeapon()
        {
            //register to owner events
            if (owner.Owner && owner.Owner.GetSavedComponent<AimComponent>())
            {
                aimComponent = owner.Owner.GetSavedComponent<AimComponent>();
                aimComponent.onChangeAimDirection += OnChangeAimDirection;

                //update order
                UpdateOrderInLayer(aimComponent.IsLookingRight);
            }
        }

        void OnDropWeapon()
        {
            //remove events from owner
            if (aimComponent)
            {
                aimComponent.onChangeAimDirection -= OnChangeAimDirection;
            }
        }

        void OnChangeAimDirection(bool isLookingRight)
        {
            //update order in layer
            UpdateOrderInLayer(isLookingRight);
        }

        #endregion
    }
}