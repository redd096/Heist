using UnityEngine;
using UnityEngine.Events;

namespace redd096
{
    [AddComponentMenu("redd096/MonoBehaviours/Event Listener")]
    public class EventListener : MonoBehaviour
    {
        [Header("Collisions")]
        [SerializeField] bool onCollisionEnter = false;
        [SerializeField] bool onTriggerEnter = false;

        [Header("Event to set")]
        [SerializeField] UnityEvent response = new UnityEvent();

        /// <summary>
        /// Call this function to invoke event
        /// </summary>
        public void InvokeEvent()
        {
            response.Invoke();
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            //call if collision enter
            if (onCollisionEnter)
            {
                InvokeEvent();
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            //call if trigger enter
            if(onTriggerEnter)
            {
                InvokeEvent();
            }
        }
    }
}