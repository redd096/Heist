using UnityEngine;
using System.Collections.Generic;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Conditions/Check Hit Something")]
    public class CheckHitSomething : ConditionTask
    {
        [Header("Necessary Components - default get in parent")]
        [SerializeField] CollisionEventToChilds collisionEventToChilds = default;
        [SerializeField] MovementComponent movementComponent = default;

        [Header("Check only in movement direction or every hit")]
        [SerializeField] bool checkMovementDirection = true;

        [Header("Ignore hit with these layers")]
        [SerializeField] VarOrBlackboard<LayerMask> layersToIgnore = default;

        [Header("Wait before start check collisions")]
        [SerializeField] VarOrBlackboard<float> delayBeforeCheck = 0.2f;

        Dictionary<GameObject, Vector2> hits = new Dictionary<GameObject, Vector2>();
        float timerBeforeCheck;

        void OnEnable()
        {
            //get references
            if (collisionEventToChilds == null)
                collisionEventToChilds = GetStateMachineComponent<CollisionEventToChilds>();

            //add events
            if (collisionEventToChilds)
            {
                collisionEventToChilds.onCollisionEnter2D += OnOwnerCollisionEnter2D;
                collisionEventToChilds.onCollisionExit2D += OnOwnerCollisionExit2D;
            }
        }

        void OnDisable()
        {
            //remove events
            if (collisionEventToChilds)
            {
                collisionEventToChilds.onCollisionEnter2D -= OnOwnerCollisionEnter2D;
                collisionEventToChilds.onCollisionExit2D -= OnOwnerCollisionExit2D;
            }
        }

        protected override void OnInitTask()
        {
            base.OnInitTask();

            //get references
            if (movementComponent == null) movementComponent = GetStateMachineComponent<MovementComponent>();
        }

        protected override void OnEnterTask()
        {
            base.OnEnterTask();

            //set timer before start check
            timerBeforeCheck = Time.time + GetValue(delayBeforeCheck);
        }

        public override bool OnCheckTask()
        {
            //before start check, wait timer
            if (timerBeforeCheck > Time.time)
                return false;

            //check hit something
            return checkMovementDirection ? CheckOnlyMovementDirection() : CheckEveryHit();
        }

        #region events

        void OnOwnerCollisionEnter2D(Collision2D collision)
        {
            //add to hits
            if (hits.ContainsKey(collision.gameObject) == false)
                hits.Add(collision.gameObject, collision.GetContact(0).point);
        }

        void OnOwnerCollisionExit2D(Collision2D collision)
        {
            //remove from hits
            if (hits.ContainsKey(collision.gameObject))
                hits.Remove(collision.gameObject);
        }

        #endregion

        #region private API

        bool CheckEveryHit()
        {
            //check if hit in some direction
            foreach (GameObject hit in hits.Keys)
            {
                //be sure is not a layer to ignore
                if (IsHit(hit))
                    return true;
            }

            //if there aren't hits or only with layers to ignore, return false
            return false;
        }

        bool CheckOnlyMovementDirection()
        {
            if (movementComponent)
            {
                if (Mathf.Abs(movementComponent.MoveDirectionInput.x) > Mathf.Epsilon)
                {
                    //check hit right
                    if (movementComponent.MoveDirectionInput.x > 0)
                    {
                        foreach (GameObject hit in hits.Keys)
                            if (IsHit(hit) && (hits[hit] - (Vector2)transform.position).normalized.x > 0)
                                return true;
                    }
                    //check hits left
                    else
                    {
                        foreach (GameObject hit in hits.Keys)
                            if (IsHit(hit) && (hits[hit] - (Vector2)transform.position).normalized.x < 0)
                                return true;
                    }
                }
                if (Mathf.Abs(movementComponent.MoveDirectionInput.y) > Mathf.Epsilon)
                {
                    //check hit up
                    if (movementComponent.MoveDirectionInput.y > 0)
                    {
                        foreach (GameObject hit in hits.Keys)
                            if (IsHit(hit) && (hits[hit] - (Vector2)transform.position).normalized.y > 0)
                                return true;
                    }
                    //check hit down
                    else
                    {
                        foreach (GameObject hit in hits.Keys)
                            if (IsHit(hit) && (hits[hit] - (Vector2)transform.position).normalized.y < 0)
                                return true;
                    }
                }
            }

            return false;
        }

        bool IsHit(GameObject hit)
        {
            //if there is not hit, or is a layer to ignore, return false
            if (hit == null || ContainsLayer(GetValue(layersToIgnore), hit.layer))
                return false;

            return true;
        }

        bool ContainsLayer(LayerMask layerMask, int layerToCompare)
        {
            //if add layer to this layermask, and layermask remain equals, then layermask contains this layer
            return layerMask == (layerMask | (1 << layerToCompare));
        }

        #endregion
    }
}