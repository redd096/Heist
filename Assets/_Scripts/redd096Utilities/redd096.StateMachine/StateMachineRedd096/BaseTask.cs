using Fusion;
using redd096.Attributes;
using UnityEngine;

namespace redd096.StateMachine.StateMachineRedd096
{
    public abstract class BaseTask : NetworkBehaviour
    {
        [Header("Task")]
        public string TaskName = "";

        public StateMachineRedd096 StateMachine { get; set; }
        public bool IsTaskActive { get; set; }

        bool isInitialized;

        /// <summary>
        /// Return StateMachine casted to inherited class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetStateMachine<T>() where T : StateMachineRedd096
        {
            return StateMachine as T;
        }

        #region protected

        //as transform use statemachine if possible, else use this task
        protected Transform transformTask => StateMachine ? StateMachine.transform : transform;

        /// <summary>
        /// Get component in parent. If not found, show warning
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="showWarningIfNotFound"></param>
        /// <returns></returns>
        protected T GetStateMachineComponent<T>(bool showWarningIfNotFound = true)
        {
            //get in parent
            T component = GetComponentInParent<T>();

            //show warning if not found
            if (showWarningIfNotFound && component == null)
                Debug.LogWarning($"Miss {typeof(T).Name} on {StateMachine}");

            return component;
        }

        /// <summary>
        /// Get blackboard or normal value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        /// <returns></returns>
        protected T GetValue<T>(VarOrBlackboard<T> v) => v != null ? v.GetValue(StateMachine) : default;

        #endregion

        #region statemachine functions

        /// <summary>
        /// Called by StateMachine, to set owner and to be sure to call OnInitTask() only one time
        /// </summary>
        /// <param name="stateMachine"></param>
        public void InitializeTask(StateMachineRedd096 stateMachine)
        {
            if (isInitialized)
                return;

            isInitialized = true;

            //set state machine and init
            this.StateMachine = stateMachine;
            OnInitTask();
        }

        /// <summary>
        /// Called by StateMachine, to set task active and call OnEnterTask()
        /// </summary>
        public void EnterTask()
        {
            IsTaskActive = true;
            OnEnterTask();
        }

        /// <summary>
        /// Called by StateMachine, to set task not active and call OnExitTask()
        /// </summary>
        public void ExitTask()
        {
            IsTaskActive = false;
            OnExitTask();
        }

        #endregion

        /// <summary>
        /// Called only one time, first time enter in this task
        /// </summary>
        protected virtual void OnInitTask() { }
        /// <summary>
        /// Called every time enter in this task
        /// </summary>
        protected virtual void OnEnterTask() { }
        /// <summary>
        /// Called every time exit from this task
        /// </summary>
        protected virtual void OnExitTask() { }
    }
}