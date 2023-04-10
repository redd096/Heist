using UnityEngine;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/StateMachine Feedback")]
    public class StateMachineFeedback : FeedbackRedd096
    {
        [Header("Necessary Components - default get in child and parent")]
        [SerializeField] Animator anim = default;
        [SerializeField] StateMachineRedd096 stateMachine = default;

        [Header("When enter in these states, call trigger in animator")]
        [SerializeField] AnimTriggerStateStruct[] animTriggers = default;

        [Header("When enter in these states, set bool in animator true or false")]
        [SerializeField] AnimBoolStateStruct[] animBools = default;

        protected override void OnEnable()
        {
            //get references
            if (anim == null) anim = GetComponentInChildren<Animator>();
            if (stateMachine == null)
            {
                Redd096Main main = GetComponentInParent<Redd096Main>();
                if (main) stateMachine = main.GetComponentInChildren<StateMachineRedd096>();
            }

            base.OnEnable();
        }

        protected override void AddEvents()
        {
            base.AddEvents();

            //add events
            if (stateMachine)
            {
                stateMachine.onSetState += OnSetState;
            }
        }

        protected override void RemoveEvents()
        {
            base.RemoveEvents();

            //remove events
            if (stateMachine)
            {
                stateMachine.onSetState -= OnSetState;
            }
        }

        void OnSetState(string stateName)
        {
            foreach (AnimTriggerStateStruct animTrigger in animTriggers)
            {
                //if enter in this state
                if (stateName.Equals(animTrigger.State))
                {
                    //set trigger
                    if (anim)
                        anim.SetTrigger(animTrigger.TriggerToSet);

                    break;
                }
            }

            foreach (AnimBoolStateStruct animBool in animBools)
            {
                //if enter in this state
                if (stateName.Equals(animBool.State))
                {
                    //set bool
                    if (anim)
                        anim.SetBool(animBool.BoolToSet, animBool.Value);

                    break;
                }
            }
        }
    }

    [System.Serializable]
    public struct AnimTriggerStateStruct
    {
        public string State;
        [Tooltip("Trigger's name in the animator")] public string TriggerToSet;
    }

    [System.Serializable]
    public struct AnimBoolStateStruct
    {
        public string State;
        [Tooltip("Bool's name in the animator")] public string BoolToSet;
        [Tooltip("Value to set when enter in this state")] public bool Value;
    }
}