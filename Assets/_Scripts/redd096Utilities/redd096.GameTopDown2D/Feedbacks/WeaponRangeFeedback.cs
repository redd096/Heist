using UnityEngine;
using redd096.Attributes;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/Weapon Range Feedback")]
    public class WeaponRangeFeedback : FeedbackRedd096<WeaponRange>
    {
        [Header("On Instantiate Bullet - use barrel where spawn bullet")]
        [SerializeField] FeedbackStructRedd096 feedbackOnInstantiateBullet = default;
        [SerializeField] CameraShakeStruct cameraShakeOnInstantiateBullet = default;
        [SerializeField] GamepadVibrationStruct gamepadVibrationOnInstantiateBullet = default;

        [Header("On Shoot - main barrel by default is transform")]
        [SerializeField] Transform mainBarrel = default;
        [SerializeField] FeedbackStructRedd096 feedbackOnShoot = default;
        [SerializeField] CameraShakeStruct cameraShakeOnShoot = default;
        [SerializeField] GamepadVibrationStruct gamepadVibrationOnShoot = default;

        [Space(20)]
        [HelpBox("A lot of times are necessary two feedbacks for OnShoot event.\n" +
            "For example first one instantiate muzzle flash that follow weapon,\n" +
            "this second one instantiate bullet shells without set parent")]
        [Header("On Shoot - second barrel by default is transform")]
        [SerializeField] Transform secondBarrel = default;
        [SerializeField] FeedbackStructRedd096 feedbackOnShoot2 = default;

        [Space(20)]
        [HelpBox("This event is called when shoot last shot of the burst.\n" +
            "To show feedback at start of the burst, use On Press Attack instead")]
        [Header("On Burst Shoot - barrel by default is transform")]
        [SerializeField] Transform barrelOnBurst = default;
        [SerializeField] FeedbackStructRedd096 feedbackOnBurst = default;
        [SerializeField] CameraShakeStruct cameraShakeOnBurst = default;
        [SerializeField] GamepadVibrationStruct gamepadVibrationOnBurst = default;

        [Header("On Press Attack - barrel by default is transform")]
        [SerializeField] Transform barrelOnPress = default;
        [SerializeField] bool deactivateInstantiatedFeedbacksOnRelease = true;
        [SerializeField] FeedbackStructRedd096 feedbackOnPress = default;
        [SerializeField] CameraShakeStruct cameraShakeOnPress = default;
        [SerializeField] GamepadVibrationStruct gamepadVibrationOnPress = default;

        [Header("On Fail Shoot - barrel by default is transform")]
        [SerializeField] Transform barrelOnFailShoot = default;
        [SerializeField] FeedbackStructRedd096 feedbackOnFailShoot = default;
        [SerializeField] CameraShakeStruct cameraShakeOnFailShoot = default;
        [SerializeField] GamepadVibrationStruct gamepadVibrationOnFailShoot = default;

        //deactive on release attack
        GameObject[] instantiatedGameObjectsOnPress;
        ParticleSystem[] instantiatedParticlesOnPress;
        AudioSource[] instantiatedAudiosOnPress;

        protected override void OnEnable()
        {
            //get references
            if (mainBarrel == null) mainBarrel = transform;
            if (secondBarrel == null) secondBarrel = transform;
            if (barrelOnBurst == null) barrelOnBurst = transform;
            if (barrelOnPress == null) barrelOnPress = transform;
            if (barrelOnFailShoot == null) barrelOnFailShoot = transform;

            base.OnEnable();
        }

        protected override void AddEvents()
        {
            //add events
            owner.onInstantiateBullet += OnInstantiateBullet;
            owner.onShoot += OnShoot;
            owner.onLastShotBurst += OnLastShotBurst;
            owner.onPressAttack += OnPressAttack;
            owner.onReleaseAttack += OnReleaseAttack;
            owner.onFailShoot += OnFailShoot;
        }

        protected override void RemoveEvents()
        {
            //remove events
            owner.onInstantiateBullet -= OnInstantiateBullet;
            owner.onShoot -= OnShoot;
            owner.onLastShotBurst -= OnLastShotBurst;
            owner.onPressAttack -= OnPressAttack;
            owner.onReleaseAttack -= OnReleaseAttack;
            owner.onFailShoot -= OnFailShoot;
        }

        #region private API

        void OnInstantiateBullet(Transform barrel)
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnInstantiateBullet, barrel);

            //camera shake and gamepad vibration
            cameraShakeOnInstantiateBullet.TryShake();
            gamepadVibrationOnInstantiateBullet.TryVibration();
        }

        void OnShoot()
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnShoot, mainBarrel);

            //camera shake and gamepad vibration
            cameraShakeOnShoot.TryShake();
            gamepadVibrationOnShoot.TryVibration();

            //instantiate second vfx and sfx
            InstantiateFeedback(feedbackOnShoot2, secondBarrel);
        }

        private void OnLastShotBurst()
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnBurst, barrelOnBurst);

            //camera shake and gamepad vibration
            cameraShakeOnBurst.TryShake();
            gamepadVibrationOnBurst.TryVibration();
        }

        void OnPressAttack()
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnPress, out instantiatedGameObjectsOnPress, out instantiatedParticlesOnPress, out instantiatedAudiosOnPress, barrelOnPress);

            //camera shake and gamepad vibration
            cameraShakeOnPress.TryShake();
            gamepadVibrationOnPress.TryVibration();
        }

        void OnReleaseAttack()
        {
            //destroy vfx and sfx instantiated on press
            if (deactivateInstantiatedFeedbacksOnRelease)
            {
                foreach (GameObject go in instantiatedGameObjectsOnPress)
                    if (go)
                        Pooling.Destroy(go);

                foreach (ParticleSystem particle in instantiatedParticlesOnPress)
                    if (particle)
                        Pooling.Destroy(particle.gameObject);

                foreach (AudioSource audio in instantiatedAudiosOnPress)
                    if (audio)
                        Pooling.Destroy(audio.gameObject);
            }
        }

        void OnFailShoot()
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnFailShoot, barrelOnFailShoot);

            //camera shake and gamepad vibration
            cameraShakeOnFailShoot.TryShake();
            gamepadVibrationOnFailShoot.TryVibration();
        }

        #endregion
    }
}