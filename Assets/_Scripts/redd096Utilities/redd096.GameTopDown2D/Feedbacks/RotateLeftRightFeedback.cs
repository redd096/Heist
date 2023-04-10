using UnityEngine;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/Rotate Left Right Feedback")]
    public class RotateLeftRightFeedback : FeedbackRedd096
    {
        [Header("Necessary Components - default get in parent")]
        [SerializeField] AimComponent aimComponent;

        [Header("Default this transform")]
        [SerializeField] Transform objectToRotate = default;
        [Tooltip("By default this object is looking to the right?")] [SerializeField] bool defaultLookRight = true;

        bool lookRight;

        void Awake()
        {
            //set default rotation
            lookRight = defaultLookRight;
        }

        protected override void OnEnable()
        {
            //get references
            if (aimComponent == null) aimComponent = GetComponentInParent<AimComponent>();
            if (objectToRotate == null) objectToRotate = transform;

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
            //if change direction, rotate
            if ((lookRight && isLookingRight == false) || (lookRight == false && isLookingRight))
            {
                if (objectToRotate) objectToRotate.rotation *= Quaternion.AngleAxis(180, Vector3.up);
                lookRight = isLookingRight;
            }
        }
    }
}