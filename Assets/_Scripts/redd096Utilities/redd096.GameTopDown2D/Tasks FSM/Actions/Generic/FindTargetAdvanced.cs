using System.Collections.Generic;
using UnityEngine;
using redd096.Attributes;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Find Target Advanced")]
    public class FindTargetAdvanced : ActionTask
    {
        [Header("Necessary Components - default get in parent")]
        [SerializeField] AimComponent aimComponent = default;

        [Header("Delay between every check (0 = every frame)")]
        [Min(0)][SerializeField] float delayBetweenChecks = 0.2f;

        [HelpBox("Targets in awareness don't need to be in cone vision, but can check if view is clear")]
        [Header("Find Target in awareness radius")]
        [SerializeField] VarOrBlackboard<LayerMask> targetLayer = default;
        [SerializeField] VarOrBlackboard<float> awarenessRadius = 5;
        [Tooltip("Check if there is a wall between this object and target")][SerializeField] bool checkViewClearForAwareness = true;

        [Header("Find Target in cone vision")]
        [SerializeField] VarOrBlackboard<float> sightDistance = 5;
        [Tooltip("Look only left and right, or use aim direction")][SerializeField] bool useOnlyLeftAndRight = false;
        [Range(1, 180)] public float viewAngle = 70f;
        [Tooltip("Check if there is a wall between this object and target")][SerializeField] bool checkViewClear = true;
        [SerializeField] VarOrBlackboard<LayerMask> layerWalls = default;
        [ColorGUI(AttributesUtility.EColor.Yellow)][SerializeField] string saveTargetInBlackboardAs = "Target";

        [Header("Call OnCompleteTask when set a target")]
        [ColorGUI(AttributesUtility.EColor.Orange)][SerializeField] bool callEvent = false;

        [Header("DEBUG")]
        [SerializeField] ShowDebugRedd096 drawAwarenessRadius = Color.cyan;
        [SerializeField] ShowDebugRedd096 drawConeVision = Color.cyan;

        List<Transform> possibleTargets = new List<Transform>();
        List<Transform> targetsInSight = new List<Transform>();
        Transform target;
        float distance;
        float timeNextCheck; //used for delays

        void OnDrawGizmos()
        {
            //draw awareness
            if (drawAwarenessRadius)
            {
                Gizmos.color = drawAwarenessRadius.ColorDebug;
                Gizmos.DrawWireSphere(transformTask.position, GetValue(awarenessRadius));
                Gizmos.color = Color.white;
            }
            //draw cone vision
            if (drawConeVision)
            {
                Gizmos.color = drawConeVision.ColorDebug;
                Vector2 direction = aimComponent ? (useOnlyLeftAndRight ? (aimComponent.IsLookingRight ? Vector2.right : Vector2.left) : aimComponent.AimDirectionInput) : Vector2.right;
                Gizmos.matrix = Matrix4x4.TRS(transformTask.position, Quaternion.LookRotation(direction), Vector3.one);

                Gizmos.DrawFrustum(Vector3.zero, viewAngle, GetValue(sightDistance), 0, 1f);

                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.color = Color.white;
            }
        }

        protected override void OnInitTask()
        {
            base.OnInitTask();

            //get references
            if (aimComponent == null) aimComponent = GetStateMachineComponent<AimComponent>();
        }

        protected override void OnEnterTask()
        {
            base.OnEnterTask();

            //reset vars
            timeNextCheck = 0;
        }

        public override void OnUpdateTask()
        {
            base.OnUpdateTask();

            if (IsWaitingDelay())
                return;

            //find every targets inside awareness and sight distance
            FindPossibleTargets();

            //check if view clear to awareness targets
            if (checkViewClearForAwareness)
            {
                foreach (Transform t in new List<Transform>(possibleTargets))
                {
                    //remove targets if can't see them
                    if (IsViewClear(t) == false)
                    {
                        possibleTargets.Remove(t);
                    }
                }
            }

            foreach (Transform t in targetsInSight)
            {
                //check if target is in cone vision (and can see them) and add to possible targets
                if (IsInsideConeVision(t) && (checkViewClear == false || IsViewClear(t)))
                {
                    possibleTargets.Add(t);
                }
            }

            if (possibleTargets.Count <= 0)
                return;

            //if found targets, save nearest in the blackboard
            GetNearest();
            StateMachine.SetBlackboardElement(saveTargetInBlackboardAs, target);

            //call event
            if (callEvent)
                CompleteTask();
        }

        #region private API

        bool IsWaitingDelay()
        {
            //if there's a delay, wait between checks
            if (delayBetweenChecks > Mathf.Epsilon && Time.time < timeNextCheck)
                return true;

            //else, set delay for next frame, but return false to do checks this frame
            timeNextCheck = Time.time + delayBetweenChecks;

            return false;
        }

        void FindPossibleTargets()
        {
            //clear lists
            possibleTargets.Clear();
            targetsInSight.Clear();

            //find every element in awareness, using layer
            foreach (Collider2D col in Physics2D.OverlapCircleAll(transformTask.position, GetValue(awarenessRadius), GetValue(targetLayer)))
            {
                possibleTargets.Add(col.transform);
            }

            //find every element in sight distance, using layer
            foreach (Collider2D col in Physics2D.OverlapCircleAll(transformTask.position, GetValue(sightDistance), GetValue(targetLayer)))
            {
                targetsInSight.Add(col.transform);
            }
        }

        bool IsViewClear(Transform t)
        {
            //check there is nothing between
            return Physics2D.Linecast(transformTask.position, t.position, GetValue(layerWalls)) == false;
        }

        bool IsInsideConeVision(Transform t)
        {
            if (aimComponent == null)
                return false;

            //check is inside view angle (use IsLookingRight to check right or left, else use aim direction input)
            return Vector2.Angle((t.position - transformTask.position).normalized, useOnlyLeftAndRight ? (aimComponent.IsLookingRight ? Vector2.right : Vector2.left) : aimComponent.AimDirectionInput) < viewAngle;
        }

        void GetNearest()
        {
            distance = Mathf.Infinity;
            target = null;

            //find nearest
            foreach (Transform t in possibleTargets)
            {
                if (Vector2.Distance(transformTask.position, t.position) < distance)
                {
                    distance = Vector2.Distance(transformTask.position, t.position);
                    target = t;
                }
            }
        }

        #endregion
    }
}