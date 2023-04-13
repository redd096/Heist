using UnityEngine;

namespace redd096.StateMachine.StateMachineRedd096
{
    [AddComponentMenu("redd096/StateMachine/StateMachineRedd096/Action Task")]
    public class ActionTask : BaseTask
    {
        /// <summary>
        /// Can call CompleteTask() to automatically call this event
        /// </summary>
        public System.Action onCompleteTask { get; set; }

        /// <summary>
        /// Called every frame when inside this task
        /// </summary>
        public virtual void OnUpdateTask() { }

        public virtual void OnFixedUpdateNetworkTask() { }

        /// <summary>
        /// Call onCompleteTask delegate
        /// </summary>
        protected void CompleteTask()
        {
            onCompleteTask?.Invoke();
        }
    }
}