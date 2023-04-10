using redd096.Attributes;
using UnityEngine;

namespace redd096.StateMachine.BasicStateMachineRedd096
{
    public abstract class State
    {
        [Header("Basic State")]
        public string StateName = "";

        public StateMachineRedd096 StateMachine { get; set; }
        public bool IsActive { get; set; }

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

        //as transform use statemachine if possible
        protected Transform transformState => StateMachine ? StateMachine.transform : null;

        /// <summary>
        /// Get component in stateMachine or parent. If not found, show warning
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="showWarningIfNotFound"></param>
        /// <returns></returns>
        protected T GetStateMachineComponent<T>(bool showWarningIfNotFound = true)
        {
            //get in parent
            T component = StateMachine ? StateMachine.GetComponentInParent<T>() : default;

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
        /// Called by StateMachine, to set owner and to be sure to call OnInit() only one time
        /// </summary>
        /// <param name="stateMachine"></param>
        public void Initialize(StateMachineRedd096 stateMachine)
        {
            if (isInitialized)
                return;

            isInitialized = true;

            //set state machine and init
            this.StateMachine = stateMachine;
            OnInit();
        }

        /// <summary>
        /// Called by StateMachine, to set active and call OnEnter()
        /// </summary>
        public void Enter()
        {
            IsActive = true;
            OnEnter();
        }

        /// <summary>
        /// Called by StateMachine, to set not active and call OnExit()
        /// </summary>
        public void Exit()
        {
            IsActive = false;
            OnExit();
        }

        /// <summary>
        /// Called by StateMachine, to call OnUpdate()
        /// </summary>
        public void Update()
        {
            OnUpdate();
        }

        /// <summary>
        /// Called by StateMachine, to call OnFixedUpdate()
        /// </summary>
        public void FixedUpdate()
        {
            OnFixedUpdate();
        }

        #endregion

        /// <summary>
        /// Called only one time, first time enter in this state
        /// </summary>
        protected virtual void OnInit() { }
        /// <summary>
        /// Called every time enter in this state
        /// </summary>
        protected virtual void OnEnter() { }
        /// <summary>
        /// Called every time exit from this state
        /// </summary>
        protected virtual void OnExit() { }
        /// <summary>
        /// Called every time update this state
        /// </summary>
        protected virtual void OnUpdate() { }
        /// <summary>
        /// Called every time fixed update this state
        /// </summary>
        protected virtual void OnFixedUpdate() { }
    }
}