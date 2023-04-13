using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace redd096.GameTopDown2D
{
    [SelectionBase]
    [AddComponentMenu("redd096/.GameTopDown2D/Main/redd096Main")]
    public class Redd096Main : NetworkBehaviour
    {
        Dictionary<System.Type, Component> components = new Dictionary<System.Type, Component>();

        /// <summary>
        /// Do GetComponent, but save the var to return next time is called
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSavedComponent<T>() where T : Component
        {
            //save in dictionary
            if (components.ContainsKey(typeof(T)) == false)
            {
                if (GetComponent<T>() == null)
                    return null;

                components.Add(typeof(T), GetComponent<T>());
            }

            //return it
            return components[typeof(T)] as T;
        }
    }
}