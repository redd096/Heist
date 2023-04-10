using UnityEngine;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/Flip Object With Character Feedback")]
    public class FlipObjectWithCharacterFeedback : FeedbackRedd096
    {
        enum EFlipType { BasedOnAim, BasedOnMovement }

        [Header("Necessary Components - default get in parent")]
        [SerializeField] MovementComponent movementComponent = default;
        [SerializeField] AimComponent aimComponent = default;

        [Header("Object to flip (already in scene/prefab) - default is this transform")]
        [SerializeField] Transform objectToFlip = default;

        [Header("Flip Rules")]
        [Tooltip("Default look right, set negative values when look left, or viceversa")][SerializeField] bool defaultCharacterIsLookingRight = true;
        [Tooltip("Use AimComponent or MovementComponent")][SerializeField] EFlipType flipType = EFlipType.BasedOnAim;
        [Tooltip("Set only position or also scale")][SerializeField] bool flipAlsoScale = false;

        bool ownerIsLookingRight;

        void Awake()
        {
            //set default value
            ownerIsLookingRight = defaultCharacterIsLookingRight;
        }

        protected override void OnEnable()
        {
            //get references
            if (movementComponent == null) movementComponent = GetComponentInParent<MovementComponent>();
            if (aimComponent == null) aimComponent = GetComponentInParent<AimComponent>();
            if (objectToFlip == null) objectToFlip = transform;

            base.OnEnable();
        }

        protected override void AddEvents()
        {
            base.AddEvents();

            //add events
            if (movementComponent)
            {
                movementComponent.onChangeMovementDirection += OnChangeMovementDirection;
                OnChangeMovementDirection(movementComponent.IsMovingRight); //set default value
            }
            if (aimComponent)
            {
                aimComponent.onChangeAimDirection += OnChangeAimDirection;
                OnChangeAimDirection(aimComponent.IsLookingRight);          //set default value
            }
        }

        protected override void RemoveEvents()
        {
            base.RemoveEvents();

            //remove events
            if (movementComponent)
            {
                movementComponent.onChangeMovementDirection -= OnChangeMovementDirection;
            }
            if (aimComponent)
            {
                aimComponent.onChangeAimDirection -= OnChangeAimDirection;
            }
        }

        #region events

        void OnChangeMovementDirection(bool movingRight)
        {
            //rotate based on movement
            if (objectToFlip && flipType == EFlipType.BasedOnMovement)
            {
                if (movingRight != ownerIsLookingRight)
                {
                    ownerIsLookingRight = movingRight;

                    //rotate scale and change offset
                    objectToFlip.localPosition = new Vector2(-objectToFlip.localPosition.x, objectToFlip.localPosition.y);
                    if (flipAlsoScale) objectToFlip.localScale = new Vector2(-objectToFlip.localScale.x, objectToFlip.localScale.y);
                }
            }
        }

        void OnChangeAimDirection(bool lookingRight)
        {
            //rotate based on aim
            if (objectToFlip && flipType == EFlipType.BasedOnAim)
            {
                if (lookingRight != ownerIsLookingRight)
                {
                    ownerIsLookingRight = lookingRight;

                    //rotate scale and change offset
                    objectToFlip.localPosition = new Vector2(-objectToFlip.localPosition.x, objectToFlip.localPosition.y);
                    if (flipAlsoScale) objectToFlip.localScale = new Vector2(-objectToFlip.localScale.x, objectToFlip.localScale.y);
                }
            }
        }

        #endregion
    }
}