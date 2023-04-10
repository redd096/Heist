using UnityEngine;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/Weapon BASE Feedback")]
    public class WeaponBASEFeedback : FeedbackRedd096<WeaponBASE>
    {
        [Header("On Pick")]
        [SerializeField] FeedbackStructRedd096 feedbackOnPick = default;
        [SerializeField] CameraShakeStruct cameraShakeOnPick = default;

        [Header("On Drop")]
        [SerializeField] FeedbackStructRedd096 feedbackOnDrop = default;
        [SerializeField] CameraShakeStruct cameraShakeOnDrop = default;

        [Header("On Equip")]
        [SerializeField] FeedbackStructRedd096 feedbackOnEquip = default;
        [SerializeField] CameraShakeStruct cameraShakeOnEquip = default;

        [Header("On Unequip")]
        [SerializeField] FeedbackStructRedd096 feedbackOnUnequip = default;
        [SerializeField] CameraShakeStruct cameraShakeOnUnequip = default;

        protected override void AddEvents()
        {
            //add events
            owner.onPickWeapon += OnPickWeapon;
            owner.onDropWeapon += OnDropWeapon;
            owner.onEquipWeapon += OnEquipWeapon;
            owner.onUnequipWeapon += OnUnequipWeapon;
        }

        protected override void RemoveEvents()
        {
            //remove events
            owner.onPickWeapon -= OnPickWeapon;
            owner.onDropWeapon -= OnDropWeapon;
            owner.onEquipWeapon -= OnEquipWeapon;
            owner.onUnequipWeapon -= OnUnequipWeapon;
        }

        #region private API

        void OnPickWeapon()
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnPick);

            //camera shake
            cameraShakeOnPick.TryShake();
        }

        void OnDropWeapon()
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnDrop);

            //camera shake
            cameraShakeOnDrop.TryShake();
        }

        void OnEquipWeapon()
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnEquip);

            //camera shake
            cameraShakeOnEquip.TryShake();
        }

        void OnUnequipWeapon()
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnUnequip);

            //camera shake
            cameraShakeOnUnequip.TryShake();
        }

        #endregion
    }
}