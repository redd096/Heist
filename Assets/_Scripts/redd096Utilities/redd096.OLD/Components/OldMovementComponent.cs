using UnityEngine;
using redd096.Attributes;

namespace redd096.OLD
{
    [AddComponentMenu("redd096/.OLD/Components/Old Movement Component")]
    public class OldMovementComponent : MonoBehaviour
    {
        enum EUpdateModes { Update, FixedUpdate }
        enum EMovementModes { Transform, Rigidbody }

        [Header("Movement")]
        [Tooltip("Move on Update or FixedUpdate?")][SerializeField] EUpdateModes updateMode = EUpdateModes.FixedUpdate;
        [Tooltip("Move using transform or rigidbody? Transform use completely CollisionComponent, rigidbody use unity physics")][SerializeField] EMovementModes movementMode = EMovementModes.Rigidbody;
        [Tooltip("Max Speed, calculating velocity by input + push (-1 = no limit)")][SerializeField] float maxSpeed = 50;

        [Header("When pushed")]
        [Tooltip("Drag based on velocity * drag or normalized velocity * drag?")][SerializeField] bool dragBasedOnVelocity = true;
        [EnableIf("movementMode", EMovementModes.Transform)][SerializeField] bool useCollisionComponent = false;
        [Tooltip("Only with CollisionComponent - When player is pushed for example to the right, and hit a wall. Set Push to right at 0?")][ShowIf("inspectorOnly_removePushForceWhenHit")][SerializeField] bool removePushForceWhenHit = false;
        [Tooltip("Use rigidbody drag or custom drag?")][SerializeField] bool useCustomDrag = false;        
        [Tooltip("Drag used when pushed from something")][ShowIf("useCustomDrag")][SerializeField] float customDrag = 5;

        [Header("Necessary Components (by default get from this gameObject)")]
        [EnableIf("movementMode", EMovementModes.Transform)][SerializeField] OldCollisionComponent collisionComponent = default;
        [EnableIf("movementMode", EMovementModes.Rigidbody)][SerializeField] Rigidbody2D rb = default;

        [Header("DEBUG")]
        [ReadOnly] public bool IsMovingRight = true;            //check if moving right
        [ReadOnly] public Vector2 MoveDirectionInput;           //when moves, set it with only input direction (used to know last movement direction)
        [ReadOnly] public Vector2 LastDesiredVelocity;          //when moves, set it as input direction * speed
        [ReadOnly] public Vector2 CurrentPushForce;             //used to push this object (push by recoil, knockback, dash, etc...), will be decreased by drag in every frame
        [ReadOnly] public Vector2 CurrentVelocity;              //calculated velocity for this frame or rigidbody.velocity
        [ReadOnly] public float CurrentSpeed;                   //CurrentVelocity.magnitude or rigidbody.velocity.magnitude
        public float MaxSpeed => maxSpeed;
        public float Drag => useCustomDrag ? customDrag : (rb ? rb.drag : 1);

        //events
        public System.Action<bool> onChangeMovementDirection { get; set; }

        //private
        Vector2 desiredVelocity;                //when moves, set it as input direction * speed (used to move this object, will be reset in every frame)
        Vector2 calculatedVelocity;             //desiredVelocity + DesiredPushForce
        Vector2 newPosition;                    //new position when Move
        Vector2 newPushForce;                   //new push force when Drag
        bool inspectorOnly_removePushForceWhenHit => movementMode == EMovementModes.Transform && useCollisionComponent;

        void Update()
        {
            //do only if update mode is Update
            if (updateMode == EUpdateModes.Update)
                Move();
        }

        void FixedUpdate()
        {
            //do only if update mode is FixedUpdate
            if (updateMode == EUpdateModes.FixedUpdate)
                Move();
        }

        void Move()
        {
            //start only if there are all necessary components
            if (CheckComponents() == false)
                return;

            //set velocity (input + push + check collisions)
            CalculateVelocity();
            CurrentVelocity = movementMode == EMovementModes.Transform ? (calculatedVelocity != Vector2.zero ? calculatedVelocity : Vector2.zero) : (rb ? rb.velocity : Vector2.zero);
            CurrentSpeed = CurrentVelocity != Vector2.zero ? CurrentVelocity.magnitude : 0.0f;

            //set if change movement direction
            if (IsMovingRight != CheckIsMovingRight())
            {
                IsMovingRight = CheckIsMovingRight();

                //call event
                onChangeMovementDirection?.Invoke(IsMovingRight);
            }

            //do movement
            DoMovement();

            //reset movement input (cause if nobody call an update, this object must to be still in next frame), but save in LastDesiredVelocity (to read in inspector or if some script need it)
            LastDesiredVelocity = desiredVelocity;
            desiredVelocity = Vector2.zero;

            //remove push force (direction * drag * delta)
            CalculateNewPushForce();
            CurrentPushForce = newPushForce;
        }

        #region private API

        bool CheckComponents()
        {
            //check if have components
            if (collisionComponent == null)
                collisionComponent = GetComponent<OldCollisionComponent>();
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            //if movement mode is transform and use CollisionComponent, be sure to have a CollisionComponent
            if (movementMode == EMovementModes.Transform && useCollisionComponent && collisionComponent == null)
            {
                Debug.LogWarning("Miss Collision Component on " + name);
                return false;
            }

            //if movement mode is rigidbody, be sure to have a rigidbody
            if (movementMode == EMovementModes.Rigidbody && rb == null)
            {
                Debug.LogWarning("Miss Rigidbody on " + name);
                return false;
            }

            return true;
        }

        void CalculateVelocity()
        {
            //input + push
            calculatedVelocity = desiredVelocity + CurrentPushForce;

            if (movementMode == EMovementModes.Transform && useCollisionComponent && collisionComponent != null)
            {
                //check if hit horizontal
                if (collisionComponent.IsHitting(OldCollisionComponent.EDirectionEnum.right) && calculatedVelocity.x > 0)
                    calculatedVelocity.x = 0;
                else if (collisionComponent.IsHitting(OldCollisionComponent.EDirectionEnum.left) && calculatedVelocity.x < 0)
                    calculatedVelocity.x = 0;

                //check if hit vertical
                if (collisionComponent.IsHitting(OldCollisionComponent.EDirectionEnum.up) && calculatedVelocity.y > 0)
                    calculatedVelocity.y = 0;
                else if (collisionComponent.IsHitting(OldCollisionComponent.EDirectionEnum.down) && calculatedVelocity.y < 0)
                    calculatedVelocity.y = 0;
            }

            //clamp at max speed
            if (maxSpeed >= 0)
                calculatedVelocity = Vector2.ClampMagnitude(calculatedVelocity, maxSpeed);
        }

        bool CheckIsMovingRight()
        {
            //check if change direction
            if (IsMovingRight && calculatedVelocity.x < 0)
                return false;
            else if (IsMovingRight == false && calculatedVelocity.x > 0)
                return true;

            //else return previous direction (necessary in case this object stay still)
            return IsMovingRight;
        }

        void DoMovement()
        {
            //do movement with rigidbody (let unity calculate reachable position)
            if (movementMode == EMovementModes.Rigidbody)
            {
                rb.velocity = calculatedVelocity;
            }
            //or move with transform (if there is collision component, calculate reachable position, else just move to new position)
            else if (movementMode == EMovementModes.Transform)
            {
                newPosition = transform.position + (Vector3)calculatedVelocity * (updateMode == EUpdateModes.Update ? Time.deltaTime : Time.fixedDeltaTime);

                //calculate reachable position
                if (useCollisionComponent && collisionComponent != null)
                {
                    if (calculatedVelocity.x > 0)
                        newPosition = collisionComponent.CalculateReachablePosition(OldCollisionComponent.EDirectionEnum.right, newPosition);
                    else if (calculatedVelocity.x < 0)
                        newPosition = collisionComponent.CalculateReachablePosition(OldCollisionComponent.EDirectionEnum.left, newPosition);

                    if (calculatedVelocity.y > 0)
                        newPosition = collisionComponent.CalculateReachablePosition(OldCollisionComponent.EDirectionEnum.up, newPosition);
                    else if (calculatedVelocity.y < 0)
                        newPosition = collisionComponent.CalculateReachablePosition(OldCollisionComponent.EDirectionEnum.down, newPosition);
                }

                transform.position = newPosition;
            }
        }

        void CalculateNewPushForce()
        {
            //remove push force (direction * drag * delta)
            newPushForce = CurrentPushForce - (
                (dragBasedOnVelocity ? CurrentPushForce : CurrentPushForce.normalized) *
                (useCustomDrag ? customDrag : (rb ? rb.drag : 1)) * 
                (updateMode == EUpdateModes.Update ? Time.deltaTime : Time.fixedDeltaTime));

            //clamp it
            if (CurrentPushForce.x >= 0 && newPushForce.x < 0 || CurrentPushForce.x <= 0 && newPushForce.x > 0)
                newPushForce.x = 0;
            if (CurrentPushForce.y >= 0 && newPushForce.y < 0 || CurrentPushForce.y <= 0 && newPushForce.y > 0)
                newPushForce.y = 0;

            //check if hit walls
            if (movementMode == EMovementModes.Transform)
            {
                if (useCollisionComponent && collisionComponent != null && removePushForceWhenHit)
                {
                    //check if hit horizontal
                    if (collisionComponent.IsHitting(OldCollisionComponent.EDirectionEnum.right) && newPushForce.x > 0)
                        newPushForce.x = 0;
                    else if (collisionComponent.IsHitting(OldCollisionComponent.EDirectionEnum.left) && newPushForce.x < 0)
                        newPushForce.x = 0;

                    //check if hit vertical
                    if (collisionComponent.IsHitting(OldCollisionComponent.EDirectionEnum.up) && newPushForce.y > 0)
                        newPushForce.y = 0;
                    else if (collisionComponent.IsHitting(OldCollisionComponent.EDirectionEnum.down) && newPushForce.y < 0)
                        newPushForce.y = 0;
                }
            }
        }

        #endregion

        #region public API

        /// <summary>
        /// Set movement in direction using custom speed
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="newSpeed"></param>
        public void MoveInDirection(Vector2 direction, float newSpeed)
        {
            //save last input direction + set movement
            MoveDirectionInput = direction.normalized;
            desiredVelocity = MoveDirectionInput * newSpeed;
        }

        /// <summary>
        /// Set movement to position using custom speed
        /// </summary>
        /// <param name="positionToReach"></param>
        /// <param name="newSpeed"></param>
        public void MoveTo(Vector2 positionToReach, float newSpeed)
        {
            //save last input direction + set movement
            MoveDirectionInput = (positionToReach - (Vector2)transform.position).normalized;
            desiredVelocity = MoveDirectionInput * newSpeed;
        }

        /// <summary>
        /// Push in direction
        /// </summary>
        /// <param name="pushDirection"></param>
        /// <param name="pushForce"></param>
        /// <param name="resetPreviousPush"></param>
        public virtual void PushInDirection(Vector2 pushDirection, float pushForce, bool resetPreviousPush = false)
        {
            //reset previous push or add new one to it
            if (resetPreviousPush)
                CurrentPushForce = pushDirection.normalized * pushForce;
            else
                CurrentPushForce += pushDirection.normalized * pushForce;
        }

        /// <summary>
        /// Set MaxSpeed at runtime
        /// </summary>
        /// <param name="maxSpeed"></param>
        public void SetMaxSpeed(float maxSpeed)
        {
            this.maxSpeed = maxSpeed;
        }

        /// <summary>
        /// Set Drag at runtime
        /// </summary>
        /// <param name="drag"></param>
        public void SetDrag(float drag)
        {
            customDrag = drag;
        }

        #endregion
    }
}