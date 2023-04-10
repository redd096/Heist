using UnityEngine;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/Pickable Feedback")]
    public class PickableFeedback : FeedbackRedd096<PickUpBASE>
    {
        [Header("On Pick")]
        [SerializeField] FeedbackStructRedd096 feedbackOnPick = default;
        [SerializeField] CameraShakeStruct cameraShakeOnPick = default;
        [SerializeField] GamepadVibrationStruct gamepadVibrationOnPick = default;

        [Header("On Fail Pick (eg. full of health)")]
        [SerializeField] FeedbackStructRedd096 feedbackOnFailPick = default;
        [SerializeField] CameraShakeStruct cameraShakeOnFailPick = default;
        [SerializeField] GamepadVibrationStruct gamepadVibrationOnFailPick = default;

        protected override void AddEvents()
        {
            base.AddEvents();

            //add events
            owner.onPick += OnPick;
            owner.onFailPick += OnFailPick;
        }

        protected override void RemoveEvents()
        {
            base.RemoveEvents();

            //remove events
            owner.onPick -= OnPick;
            owner.onFailPick -= OnFailPick;
        }

        void OnPick(PickUpBASE obj)
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnPick);

            //camera shake and gamepad vibration
            cameraShakeOnPick.TryShake();
            gamepadVibrationOnPick.TryVibration();
        }

        void OnFailPick(PickUpBASE obj)
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnFailPick);

            //camera shake and gamepad vibration
            cameraShakeOnFailPick.TryShake();
            gamepadVibrationOnFailPick.TryVibration();
        }
    }
}