using UnityEngine;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/Get Hit Animation Feedback")]
    public class GetHitAnimationFeedback : FeedbackRedd096
    {
        [Header("Necessary Components - default get in child and parent")]
        [SerializeField] Animator anim = default;
        [SerializeField] HealthComponent healthComponent = default;

        [Header("Animator triggers' name")]
        [SerializeField] string hitTrigger = "Damage";
        [SerializeField] string deathTrigger = "Dead";

        [Header("Checks")]
        [SerializeField] bool setTriggerOnGetDamage = true;
        [SerializeField] bool setTriggerOnDie = true;

        protected override void OnEnable()
        {
            //get references
            if (anim == null) anim = GetComponentInChildren<Animator>();
            if (healthComponent == null) healthComponent = GetComponentInParent<HealthComponent>();

            base.OnEnable();
        }

        protected override void AddEvents()
        {
            base.AddEvents();

            //add events
            if (healthComponent)
            {
                healthComponent.onGetDamage += OnGetDamage;
                healthComponent.onDie += OnDie;
            }
        }

        protected override void RemoveEvents()
        {
            base.RemoveEvents();

            //remove events
            if (healthComponent)
            {
                healthComponent.onGetDamage -= OnGetDamage;
                healthComponent.onDie -= OnDie;
            }
        }

        void OnGetDamage(Character whoHit, Vector2 hitPoint)
        {
            //set trigger on get damage
            if (anim && setTriggerOnGetDamage)
                anim.SetTrigger(hitTrigger);
        }

        void OnDie(HealthComponent healthComponent, Character whoHit)
        {
            //set trigger on die
            if (anim && setTriggerOnDie)
                anim.SetTrigger(deathTrigger);
        }
    }
}