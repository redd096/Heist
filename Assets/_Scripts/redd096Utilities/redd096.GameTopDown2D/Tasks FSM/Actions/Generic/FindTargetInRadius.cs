using System.Collections.Generic;
using UnityEngine;
using redd096.Attributes;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Find Target In Radius")]
    public class FindTargetInRadius : ActionTask
    {
        [Header("Delay between every check (0 = every frame)")]
        [Min(0)][SerializeField] float delayBetweenChecks = 0.2f;

        [Header("Find Target in radius")]
        [SerializeField] VarOrBlackboard<LayerMask> targetLayer = default;
        [SerializeField] VarOrBlackboard<float> radius = 5;

        [Header("Check if there is a wall between this object and target")]
        [SerializeField] bool checkViewClear = true;
        [SerializeField] VarOrBlackboard<LayerMask> layerWalls = default;
        [ColorGUI(AttributesUtility.EColor.Yellow)][SerializeField] string saveTargetInBlackboardAs = "Target";

        [Header("Call OnCompleteTask when set a target")]
        [ColorGUI(AttributesUtility.EColor.Orange)][SerializeField] bool callEvent = false;

        [Header("DEBUG")]
        [SerializeField] ShowDebugRedd096 drawRadius = Color.cyan;

        List<Transform> possibleTargets = new List<Transform>();
        Transform target;
        float distance;
        float timeNextCheck; //used for delays

        void OnDrawGizmos()
        {
            //draw radius
            if (drawRadius)
            {
                Gizmos.color = drawRadius.ColorDebug;
                Gizmos.DrawWireSphere(transformTask.position, GetValue(radius));
                Gizmos.color = Color.white;
            }
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

            //find every targets inside radius
            FindPossibleTargets();

            //check if view clear to possible targets
            if (checkViewClear)
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
            //clear list
            possibleTargets.Clear();

            //find every element in distance, using layer
            foreach (Collider2D col in Physics2D.OverlapCircleAll(transformTask.position, GetValue(radius), GetValue(targetLayer)))
            {
                possibleTargets.Add(col.transform);
            }
        }

        bool IsViewClear(Transform t)
        {
            //check there is nothing between
            return Physics2D.Linecast(transformTask.position, t.position, GetValue(layerWalls)) == false;
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