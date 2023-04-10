using System.Collections.Generic;
using UnityEngine;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Components/Enemies/Damage Character On Hit")]
    public class DamageCharacterOnHit : MonoBehaviour
    {
        [Header("Do Damage On Hit? - Collision or Trigger?")]
        [SerializeField] bool useTrigger = false;

        [Header("Damage")]
        [SerializeField] List<Character.ECharacterType> charactersToHit = new List<Character.ECharacterType>() { Character.ECharacterType.Player };
        [SerializeField] float damage = 10;
        [SerializeField] float pushForce = 10;
        [Tooltip("Necessary for on collision stay, to not call damage every frame")][SerializeField] float delayBetweenAttacks = 1;

        //events
        public System.Action onHit { get; set; }

        //set if do damage or not
        bool canDoDamage = true;

        Character selfCharacter;
        Dictionary<Character, float> hits = new Dictionary<Character, float>();
        Character hitCharacter;

        void Start()
        {
            //get references
            selfCharacter = GetComponent<Character>();
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (useTrigger && CheckCollisionEnter(collision.transform))
            {
                //damage it and add to the list
                OnHit(hitCharacter, collision.ClosestPoint(transform.position));
                hits.Add(hitCharacter, Time.time + delayBetweenAttacks);        //set timer
            }
        }

        void OnTriggerStay2D(Collider2D collision)
        {
            if (useTrigger && CheckCollisionStay(collision.transform))
            {
                OnHit(hitCharacter, collision.ClosestPoint(transform.position));
                hits[hitCharacter] = Time.time + delayBetweenAttacks;           //reset timer
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (useTrigger && CheckCollisionExit(collision.transform))
            {
                //remove from the list
                hits.Remove(hitCharacter);
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (useTrigger == false && CheckCollisionEnter(collision.transform))
            {
                //damage it and add to the list
                OnHit(hitCharacter, collision.GetContact(0).point);
                hits.Add(hitCharacter, Time.time + delayBetweenAttacks);        //set timer
            }
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (useTrigger == false && CheckCollisionStay(collision.transform))
            {
                OnHit(hitCharacter, collision.GetContact(0).point);
                hits[hitCharacter] = Time.time + delayBetweenAttacks;           //reset timer
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (useTrigger == false && CheckCollisionExit(collision.transform))
            {
                //remove from the list
                hits.Remove(hitCharacter);
            }
        }

        #region private API

        bool CheckCollisionEnter(Transform collision)
        {
            //do nothing if deactivated (use boolean because the unity toggle doesn't work with collision enter)
            if (enabled == false)
                return false;

            //check if hit character and is not already in the list
            hitCharacter = collision.GetComponentInParent<Character>();
            if (hitCharacter && hits.ContainsKey(hitCharacter) == false)
            {
                //check can damage
                if ((selfCharacter == null || hitCharacter != selfCharacter) &&     //be sure to not hit self
                    charactersToHit.Contains(hitCharacter.CharacterType))           //and be sure is type can hit
                {
                    return true;
                }
            }

            return false;
        }

        bool CheckCollisionStay(Transform collision)
        {
            //do nothing if deactivated (use boolean because the unity toggle doesn't work with collision enter)
            if (enabled == false)
                return false;

            //check if hit character and is in the list
            hitCharacter = collision.transform.GetComponentInParent<Character>();
            if (hitCharacter && hits.ContainsKey(hitCharacter))
            {
                //damage after delay
                if (Time.time > hits[hitCharacter])
                {
                    return true;
                }
            }

            return false;
        }

        bool CheckCollisionExit(Transform collision)
        {
            //do nothing if deactivated (use boolean because the unity toggle doesn't work with collision enter)
            if (enabled == false)
                return false;

            //check if exit hit character and is in the list
            hitCharacter = collision.transform.GetComponentInParent<Character>();
            if (hitCharacter && hits.ContainsKey(hitCharacter))
            {
                return true;
            }

            return false;
        }

        #endregion

        void OnHit(Character hit, Vector2 hitPoint)
        {
            //if can't do damage, return, but continue to check if enter or exit collision
            if (canDoDamage == false)
                return;

            //if there is no character, do nothing
            if (hit == null)
            {
                //and remove from the list
                if (hits.ContainsKey(hit))
                    hits.Remove(hit);

                return;
            }

            //call event
            onHit?.Invoke();

            //do damage and push back
            if (hit.GetSavedComponent<HealthComponent>())
                hit.GetSavedComponent<HealthComponent>().GetDamage(damage, selfCharacter, hitPoint);

            if (hit && hit.GetSavedComponent<MovementComponent>())
                hit.GetSavedComponent<MovementComponent>().PushInDirection(((Vector2)hit.transform.position - hitPoint).normalized, pushForce);
        }

        #region public API

        /// <summary>
        /// Set if can do damage or not
        /// </summary>
        /// <param name="canDoDamage"></param>
        public void SetCanDoDamage(bool canDoDamage)
        {
            this.canDoDamage = canDoDamage;
        }

        /// <summary>
        /// Get if can do damage or not
        /// </summary>
        /// <returns></returns>
        public bool GetCanDoDamage()
        {
            return canDoDamage;
        }

        #endregion
    }
}