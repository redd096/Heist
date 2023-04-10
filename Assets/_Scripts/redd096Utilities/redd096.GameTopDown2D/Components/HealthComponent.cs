using UnityEngine;
using redd096.Attributes;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Components/Health Component")]
    public class HealthComponent : MonoBehaviour
    {
        [Header("Health")]
        [Tooltip("Used to set permanent invincible. To check if is currently invincible use IsCurrentlyInvincible, because that check also if temporary invincible")] public bool Invincible = false;
        [OnValueChanged("OnChangeMaxHealth")] public float MaxHealth = 100;

        [Header("Temp invincible when hitted")]
        public bool SetTempInvincibleWhenHitted = true;
        [EnableIf("SetTempInvincibleWhenHitted")] public float DurationTempInvincibilityWhenHitted = 0.5f;

        [Header("Friendly Fire")]
        [Range(0f, 1f)][SerializeField] float percentageDamageWhenHitFromFriend = 0.25f;

        [Header("Destroy object when dead")]
        [SerializeField] bool destroyOnDie = true;
        [EnableIf("destroyOnDie")][SerializeField] float timeBeforeDestroy = 0;

        [Header("DEBUG")]
        [ProgressBar("Health", "MaxHealth", AttributesUtility.EColor.SmoothGreen)] public float CurrentHealth = 100;
        /*[ShowNonSerializedField]*/ public bool IsCurrentlyInvincible => Invincible || Time.time < tempInvincibleTime;
        /*[ShowNonSerializedField]*/ public bool IsCurrentlyTemporaryInvincible => Time.time < tempInvincibleTime;
        /*[ShowNonSerializedField]*/ [ReadOnly] bool alreadyDead = false;

        //events
        public System.Action<Character, Vector2> onGetDamage { get; set; }
        public System.Action<HealthComponent, Character> onDie { get; set; }
        public System.Action onGetHealth { get; set; }
        public System.Action onChangeHealth { get; set; }
        public System.Action onSetTemporaryInvincible { get; set; }

        Character ownerCharacter;
        Shield shield;
        float tempInvincibleTime;

        void OnChangeMaxHealth()
        {
            if (Application.isPlaying == false)
                CurrentHealth = MaxHealth;
        }

        void Awake()
        {
            //get references
            ownerCharacter = GetComponent<Character>();
            shield = GetComponent<Shield>();
        }

        /// <summary>
        /// Get damage and check if dead
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="whoHit"></param>
        /// <param name="hitPoint"></param>
        /// <param name="ignoreShield"></param>
        public void GetDamage(float damage, Character whoHit, Vector2 hitPoint, bool ignoreShield = false)
        {
            if (alreadyDead)
                return;

            //if hitted from same type of character, decrease damage
            if (HittedFromSameCharacterType(whoHit))
            {
                damage *= percentageDamageWhenHitFromFriend;
            }

            //check if hit shield, decrease damage
            if (HittedOnTheShield(hitPoint, ignoreShield))
            {
                damage *= shield.PercentageDamage();
            }

            //set health, only if not invincible
            if (Invincible == false && Time.time > tempInvincibleTime)
            {
                CurrentHealth -= damage;

                //set also temporary invincible
                if (SetTempInvincibleWhenHitted)
                    SetTemporaryInvincible(DurationTempInvincibilityWhenHitted);

                //call event only if damage is > 0
                if (damage > Mathf.Epsilon)
                {
                    onGetDamage?.Invoke(whoHit, hitPoint);
                }

                //call always event
                onChangeHealth?.Invoke();
            }

            //check if dead
            if (CurrentHealth <= 0)
            {
                Die(whoHit);
            }
        }

        /// <summary>
        /// Call it when health reach 0
        /// </summary>
        /// <param name="whoHit"></param>
        public void Die(Character whoHit)
        {
            if (alreadyDead)
                return;

            alreadyDead = true;

            //call event
            onDie?.Invoke(this, whoHit);

            //destroy object (if necessary)
            if (destroyOnDie)
            {
                Destroy(gameObject, timeBeforeDestroy);
            }
        }

        /// <summary>
        /// Get health and clamp to max health
        /// </summary>
        /// <param name="healthGiven"></param>
        /// <param name="clampMaxHealth">Clamp or can exceed max health?</param>
        public void GetHealth(float healthGiven, bool clampMaxHealth = true)
        {
            if (alreadyDead)
                return;

            //add health
            CurrentHealth += healthGiven;

            //clamp to max health if necessary
            if (clampMaxHealth && CurrentHealth > MaxHealth)
                CurrentHealth = MaxHealth;

            //call events
            onGetHealth?.Invoke();
            onChangeHealth?.Invoke();
        }

        /// <summary>
        /// Set invincible for few seconds
        /// </summary>
        /// <param name="duration"></param>
        public void SetTemporaryInvincible(float duration)
        {
            //set invincible only if greater than currently temporary invincibility
            if (Time.time + duration > tempInvincibleTime)
            {
                tempInvincibleTime = Time.time + duration;
                onSetTemporaryInvincible?.Invoke();
            }
        }

        #region private API

        bool HittedFromSameCharacterType(Character whoHit)
        {
            //check if both are character
            if (ownerCharacter && whoHit)
            {
                //check if they are same type of character
                return ownerCharacter.CharacterType == whoHit.CharacterType;
            }

            return false;
        }

        bool HittedOnTheShield(Vector2 hitPoint, bool ignoreShield)
        {
            //return false if this hit ignore shield
            if (ignoreShield)
                return false;

            //else return if we have a shield and hit point is on it
            return shield && shield.CheckHitShield(hitPoint);
        }

        #endregion
    }
}