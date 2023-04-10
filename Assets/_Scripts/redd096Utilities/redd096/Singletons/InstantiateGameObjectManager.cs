using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace redd096
{
    [System.Serializable]
    public struct InstantiatedGameObjectStruct
    {
        public GameObject instantiatedGameObject;
        [Tooltip("0 = no autodestruction")] public float timeAutodestruction;
    }

    [AddComponentMenu("redd096/Singletons/Instantiate GameObject Manager")]
    public class InstantiateGameObjectManager : Singleton<InstantiateGameObjectManager>
    {
        private Transform parent;
        public Transform Parent
        {
            get
            {
                if (parent == null)
                    parent = new GameObject("Instantiated Game Objects Parent").transform;

                return parent;
            }
        }
        Dictionary<GameObject, Pooling<GameObject>> pooling = new Dictionary<GameObject, Pooling<GameObject>>();

        /// <summary>
        /// Spawn at point and rotation. Use specific pooling
        /// </summary>
        public GameObject Play(Pooling<GameObject> pool, GameObject prefab, Vector3 position, Quaternion rotation, float timeAutodestruction)
        {
            if (prefab == null)
                return null;

            //instantiate (if didn't find deactivated, take first one in the pool)
            GameObject element = pool.Instantiate(prefab);
            if (element == null && pool.PooledObjects.Count > 0)
                element = pool.PooledObjects[0];

            //if still null, return
            if (element == null)
                return null;

            //set position, rotation and parent
            element.transform.position = position;
            element.transform.rotation = rotation;
            element.transform.SetParent(Parent);

            //start coroutine to deactivate
            if (timeAutodestruction > 0)
                StartCoroutine(DeactiveAfterSeconds(element, timeAutodestruction));

            return element;
        }

        IEnumerator DeactiveAfterSeconds(GameObject gameObjectToDeactivate, float timeAutodestruction)
        {
            //wait
            yield return new WaitForSeconds(timeAutodestruction);

            //and deactive
            if (gameObjectToDeactivate)
                gameObjectToDeactivate.gameObject.SetActive(false);
        }

        /// <summary>
        /// Spawn at point and rotation
        /// </summary>
        public GameObject Play(InstantiatedGameObjectStruct prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab.instantiatedGameObject == null)
                return null;

            //if this pooling is not in the dictionary, add it
            if (pooling.ContainsKey(prefab.instantiatedGameObject) == false)
                pooling.Add(prefab.instantiatedGameObject, new Pooling<GameObject>());

            //use this manager's pooling, instead of a specific one
            return Play(pooling[prefab.instantiatedGameObject], prefab.instantiatedGameObject, position, rotation, prefab.timeAutodestruction);
        }

        /// <summary>
        /// Spawn at point and rotation. Get one random from the array
        /// </summary>
        public GameObject Play(InstantiatedGameObjectStruct[] prefabs, Vector3 position, Quaternion rotation)
        {
            //do only if there are elements in the array
            if (prefabs.Length > 0)
            {
                return Play(prefabs[Random.Range(0, prefabs.Length)], position, rotation);
            }

            return null;
        }
    }
}