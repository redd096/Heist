using UnityEngine;

namespace redd096
{
    [DefaultExecutionOrder(-10)]
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T instance { get; private set; }

        protected virtual void Awake()
        {
            CheckInstance();
        }

        void CheckInstance()
        {
            if (instance)
            {
                //if there is already an instance, destroy this one
                Destroy(gameObject);
            }
            else
            {
                //else, set this as unique instance and set don't destroy on load
                instance = (T)this;
                DontDestroyOnLoad(this);
            }

            //call set defaults in the instance
            instance.SetDefaults();
        }

        /// <summary>
        /// Called on Awake. This will be called every time is loaded a new scene, if in the new scene there is another object of this type
        /// </summary>
        protected virtual void SetDefaults()
        {
            //things you must to call on every awake 
            //(every change of scene where there is another instance of this object)
        }
    }
}