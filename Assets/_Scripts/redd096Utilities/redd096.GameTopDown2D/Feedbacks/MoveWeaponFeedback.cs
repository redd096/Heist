using UnityEngine;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/Move Weapon Feedback")]
    public class MoveWeaponFeedback : FeedbackRedd096<WeaponBASE>
    {
        [Header("Pivot - default is this transform")]
        [SerializeField] Transform objectPivot = default;
        [SerializeField] float offsetFromCharacter = 0.5f;

        protected override void OnEnable()
        {
            //get references
            if (objectPivot == null) objectPivot = transform;

            base.OnEnable();
        }

        void Update()
        {
            //set position using Aim
            MoveWeapon();
        }

        void MoveWeapon()
        {
            //move to owner + offset
            if (owner && owner.Owner && owner.Owner.GetSavedComponent<AimComponent>())
            {
                objectPivot.position = (Vector2)owner.Owner.transform.position + (owner.Owner.GetSavedComponent<AimComponent>().AimDirectionInput * offsetFromCharacter);
            }
        }
    }
}