using System.Collections;
using UnityEngine;

namespace redd096.GameTopDown2D
{
    /// <summary>
    /// Can use RotateLeftRightFeedback if prefer to rotate instead of flip sprite
    /// </summary>
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/Flip Sprite Feedback")]
    public class FlipSpriteFeedback : FeedbackRedd096
    {
        [Header("Necessary Components - default get in parent")]
        [SerializeField] AimComponent aimComponent;

        [Header("Default get component in children")]
        [Tooltip("By default these sprites are looking to the right?")][SerializeField] bool defaultLookRight = true;
        [SerializeField] SpriteRenderer[] spritesToFlip = default;

        //TEMP
        Coroutine flipSpriteCoroutine;
        Character selfCharacter;

        protected override void OnEnable()
        {
            //TEMP
            if (selfCharacter == null) selfCharacter = GetComponentInParent<Character>();

            //get references
            if (aimComponent == null) aimComponent = GetComponentInParent<AimComponent>();
            if (spritesToFlip == null || spritesToFlip.Length <= 0) spritesToFlip = GetComponentsInChildren<SpriteRenderer>();

            base.OnEnable();
        }

        protected override void AddEvents()
        {
            base.AddEvents();

            //add events
            if (aimComponent)
            {
                aimComponent.onChangeAimDirection += OnChangeAimDirection;
                OnChangeAimDirection(aimComponent.IsLookingRight);      //set default rotation
            }
        }

        protected override void RemoveEvents()
        {
            base.RemoveEvents();

            //remove events
            if (aimComponent)
                aimComponent.onChangeAimDirection -= OnChangeAimDirection;
        }

        //public to use for example when doing Dash. Disable this script and call manually this function
        public void OnChangeAimDirection(bool isLookingRight)
        {
            //TEMP for enemies use a coroutine
            if (selfCharacter && selfCharacter.CharacterType == Character.ECharacterType.AI)
            {
                //restart coroutine
                if (flipSpriteCoroutine != null)
                    StopCoroutine(flipSpriteCoroutine);

                flipSpriteCoroutine = StartCoroutine(FlipSpriteCoroutine());
                return;
            }

            //flip right or left
            foreach (SpriteRenderer sprite in spritesToFlip)
                if (sprite)
                    sprite.flipX = (defaultLookRight && isLookingRight == false) || (defaultLookRight == false && isLookingRight);
        }

        //TEMP - is used from enemies because now they receive path after few frames and can have rotation problems
        IEnumerator FlipSpriteCoroutine()
        {
            //wait a bit before rotate
            yield return new WaitForSeconds(0.1f);

            //flip right or left
            if (aimComponent)
                foreach (SpriteRenderer sprite in spritesToFlip)
                    if (sprite)
                        sprite.flipX = (defaultLookRight && aimComponent.IsLookingRight == false) || (defaultLookRight == false && aimComponent.IsLookingRight);
        }
    }
}