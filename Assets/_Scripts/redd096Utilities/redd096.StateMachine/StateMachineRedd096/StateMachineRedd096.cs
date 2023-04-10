using System.Collections.Generic;
using UnityEngine;
using redd096.Attributes;

namespace redd096.StateMachine.StateMachineRedd096
{
    [AddComponentMenu("redd096/StateMachine/StateMachineRedd096/State Machine redd096")]
    public class StateMachineRedd096 : MonoBehaviour
    {
        public State[] States = default;

        [Header("DEBUG")]
        [ReadOnly] public State CurrentState = default;
        [ReadOnly][SerializeField] List<string> blackboardDebug = new List<string>();

        //blackboard to save vars to use in differents tasks
        Dictionary<string, object> blackboard = new Dictionary<string, object>();

        //events
        public System.Action<string> onSetState { get; set; }
        public System.Action<string> onSetBlackboardValue { get; set; }

        protected virtual void Start()
        {
            //start with first state
            SetState(0);
        }

        protected virtual void Update()
        {
            if (CurrentState != null)
            {
                //do every action
                DoActions();

                //check every transition
                CheckTransitions();
            }
        }

        #region public API

        /// <summary>
        /// Exit from previous state and enter in new one
        /// </summary>
        /// <param name="nextState"></param>
        public virtual void SetState(int nextState)
        {
            //exit from previous state
            if (CurrentState != null)
            {
                ExitState();
            }

            //set new state
            CurrentState = nextState >= 0 && States != null && States.Length > nextState ? States[nextState] : null;

            //enter in new state
            if (CurrentState != null)
            {
                EnterState();
            }

            //call event
            onSetState?.Invoke(CurrentState != null ? CurrentState.StateName : string.Empty);
        }

        /// <summary>
        /// Exit from previous state and enter in new one
        /// </summary>
        /// <param name="nextState"></param>
        public virtual void SetState(string nextState)
        {
            if (States != null)
            {
                //find state name and set it
                for (int i = 0; i < States.Length; i++)
                {
                    if (nextState.Equals(States[i].StateName))//, System.StringComparison.CurrentCultureIgnoreCase)
                    {
                        SetState(i);
                        return;
                    }
                }
            }

            //else set null as state
            SetState(-1);
        }

        /// <summary>
        /// Get current state name. If state is null, return empty string
        /// </summary>
        /// <returns></returns>
        public string GetCurrentState()
        {
            return CurrentState != null ? CurrentState.StateName : string.Empty;
        }

        #endregion

        #region blackboard public API

        /// <summary>
        /// Add or Set element inside blackboard
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetBlackboardElement(string key, object value)
        {
            //add element if not inside blackboard
            if (blackboard.ContainsKey(key) == false)
            {
                blackboard.Add(key, value);
            }
            //else set it
            else
            {
                blackboard[key] = value;
            }

            //in editor, update debug blackboard
            if (Application.isEditor)
            {
                //add value in blackboard
                if (blackboardDebug.Contains(key) == false)
                {
                    blackboardDebug.Add(key);
                }
            }

            //call event
            onSetBlackboardValue?.Invoke(key);
        }

        /// <summary>
        /// Remove element from blackboard
        /// </summary>
        /// <param name="key"></param>
        public void RemoveBlackboardElement(string key)
        {
            //remove element from blackboard
            if (blackboard.ContainsKey(key))
                blackboard.Remove(key);

            //in editor, update debug blackboard
            if (Application.isEditor)
            {
                //remove value from blackboard
                if (blackboardDebug.Contains(key))
                {
                    blackboardDebug.Remove(key);
                }
            }
        }

        /// <summary>
        /// Get element from blackboard
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetBlackboardElement<T>(string key)
        {
            //return element from blackboard
            if (blackboard.ContainsKey(key))
            {
                return (T)blackboard[key];
            }

            //if there is no key, return null
            return default;
        }

        #endregion

        #region private API

        void DoActions()
        {
            if (CurrentState.Actions == null)
                return;

            //do every action
            for (int i = 0; i < CurrentState.Actions.Length; i++)
            {
                if (CurrentState.Actions[i] != null)
                    CurrentState.Actions[i].OnUpdateTask();
            }
        }

        void CheckTransitions()
        {
            if (CurrentState.Transitions == null)
                return;

            //check every transition
            for (int i = 0; i < CurrentState.Transitions.Length; i++)
            {
                if (CurrentState.Transitions[i] != null)
                {
                    //if conditions return true, set new state
                    if ((CurrentState.Transitions[i].TransitionCheck == ETransitionCheck.AllTrueRequired && CheckConditionsEVERY(CurrentState.Transitions[i]))
                        || (CurrentState.Transitions[i].TransitionCheck == ETransitionCheck.AnyTrueSuffice && CheckConditionsANY(CurrentState.Transitions[i])))
                    {
                        SetState(CurrentState.Transitions[i].DestinationState);

                        //break, because now is in another state
                        break;
                    }
                }
            }
        }

        bool CheckConditionsEVERY(Transition transition)
        {
            if (transition.Conditions == null)
                return false;

            //return true if EVERY condition is true
            for (int i = 0; i < transition.Conditions.Count; i++)
            {
                if (transition.Conditions[i] != null)
                {
                    //if anyone is false, return false
                    if (transition.Conditions[i].OnCheckTask() == false)
                        return false;
                }
            }

            return true;
        }

        bool CheckConditionsANY(Transition transition)
        {
            if (transition.Conditions == null)
                return false;

            //return true if ANY condition is true
            for (int i = 0; i < transition.Conditions.Count; i++)
            {
                if (transition.Conditions[i] != null)
                {
                    //if anyone is true, return true
                    if (transition.Conditions[i].OnCheckTask())
                        return true;
                }
            }

            return false;
        }

        void ExitState()
        {
            //exit from actions
            if (CurrentState.Actions != null)
            {
                foreach (ActionTask action in CurrentState.Actions)
                    if (action)
                        action.ExitTask();
            }

            //exit from previous conditions
            if (CurrentState.Transitions != null)
            {
                foreach (Transition transition in CurrentState.Transitions)
                    if (transition != null && transition.Conditions != null)
                        foreach (ConditionTask condition in transition.Conditions)
                            if (condition)
                                condition.ExitTask();
            }
        }

        void EnterState()
        {
            //enter in new actions
            if (CurrentState.Actions != null)
            {
                foreach (ActionTask action in CurrentState.Actions)
                {
                    if (action)
                    {
                        action.InitializeTask(this);
                        action.EnterTask();
                    }
                }
            }

            //enter in new conditions
            if (CurrentState.Transitions != null)
            {
                foreach (Transition transition in CurrentState.Transitions)
                {
                    if (transition != null && transition.Conditions != null)
                    {
                        foreach (ConditionTask condition in transition.Conditions)
                        {
                            if (condition)
                            {
                                condition.InitializeTask(this);
                                condition.EnterTask();
                            }
                        }
                    }
                }
            }

        }

        #endregion
    }

    #region classes and enums

    public enum ETransitionCheck { AllTrueRequired, AnyTrueSuffice }

    [System.Serializable]
    public class Transition
    {
        [DropdownState] public string DestinationState;
        public ETransitionCheck TransitionCheck;
        [DropdownTask(typeof(ConditionTask))] public List<ConditionTask> Conditions;
    }

    [System.Serializable]
    public class State
    {
        [OnStateNameChanged] public string StateName;
        [DropdownTask(typeof(ActionTask))] public ActionTask[] Actions;
        public Transition[] Transitions;
    }

    #endregion
}