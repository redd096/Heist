using UnityEngine;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/Bullet Feedback")]
    public class BulletFeedback : FeedbackRedd096<Bullet>
    {
        [Header("On Spawn bullet")]
        [SerializeField] FeedbackStructRedd096 feedbackOnInit = default;
        [SerializeField] CameraShakeStruct cameraShakeOnInit = default;

        [Header("On Hit (also if move through this object)")]
        [SerializeField] FeedbackStructRedd096 feedbackOnHit = default;
        [SerializeField] CameraShakeStruct cameraShakeOnHit = default;

        [Header("On Bounce")]
        [SerializeField] FeedbackStructRedd096 feedbackOnBounce = default;
        [SerializeField] CameraShakeStruct cameraShakeOnBounce = default;

        [Header("On Hit something that destroy bullet")]
        [SerializeField] FeedbackStructRedd096 feedbackOnLastHit = default;
        [SerializeField] CameraShakeStruct cameraShakeOnLastHit = default;

        [Header("On AutoDestruction")]
        [SerializeField] FeedbackStructRedd096 feedbackOnAutoDestruction = default;
        [SerializeField] CameraShakeStruct cameraShakeOnAutoDestruction = default;

        [Header("On Destroy (both autodestruction or hit)")]
        [SerializeField] FeedbackStructRedd096 feedbackOnDestroy = default;
        [SerializeField] CameraShakeStruct cameraShakeOnDestroy = default;

        TrailRenderer trail;

        protected override void OnEnable()
        {
            //get references
            if (trail == null)
                trail = GetComponentInChildren<TrailRenderer>();

            base.OnEnable();
        }

        protected override void AddEvents()
        {
            base.AddEvents();

            //add events
            owner.onInit += OnInit;
            owner.onHit += OnHit;
            owner.onBounceHit += OnBounceHit;
            owner.onLastHit += OnLastHit;
            owner.onAutodestruction += OnAutodestruction;
            owner.onDie += OnDie;
        }

        protected override void RemoveEvents()
        {
            base.RemoveEvents();

            //remove events
            owner.onInit -= OnInit;
            owner.onHit -= OnHit;
            owner.onBounceHit -= OnBounceHit;
            owner.onLastHit -= OnLastHit;
            owner.onAutodestruction -= OnAutodestruction;
            owner.onDie -= OnDie;
        }

        private void OnInit()
        {
            //reset trail
            if (trail)
                trail.Clear();

            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnInit);

            //camera shake
            cameraShakeOnInit.TryShake();
        }

        private void OnHit(GameObject hit)
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnHit);

            //camera shake
            cameraShakeOnHit.TryShake();
        }

        private void OnBounceHit(GameObject obj)
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnBounce);

            //camera shake
            cameraShakeOnBounce.TryShake();
        }

        private void OnLastHit(GameObject hit)
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnLastHit);

            //camera shake
            cameraShakeOnLastHit.TryShake();
        }

        private void OnAutodestruction()
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnAutoDestruction);

            //camera shake
            cameraShakeOnAutoDestruction.TryShake();
        }

        private void OnDie(Bullet bullet)
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnDestroy);

            //camera shake
            cameraShakeOnDestroy.TryShake();
        }
    }
}