using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace redd096
{
    [AddComponentMenu("redd096/Procedural Map/Controller Procedural Map Managers")]
    public class ControllerProceduralMapManagers : MonoBehaviour
    {
        [Header("Managers to activate in order")]
        [SerializeField] protected ProceduralMapManager[] mapManagers = default;

        public List<Room> roomsEveryMapManager { get; private set; } = new List<Room>();

        void Start()
        {
            StartCoroutine(StartManagers());
        }

        IEnumerator StartManagers()
        {
            //destroy every map by default
            foreach (ProceduralMapManager mapManager in mapManagers)
            {
                if (mapManager)
                    mapManager.DestroyMap();
            }

            //foreach map manager, create map
            for (int i = 0; i < mapManagers.Length; i++)
            {
                if (mapManagers[i])
                {
                    yield return mapManagers[i].CreateMap(roomsEveryMapManager);

                    //add created rooms to controller
                    roomsEveryMapManager.AddRange(mapManagers[i].Rooms);
                }
            }

            //end generation for every map manager
            foreach (ProceduralMapManager mapManager in mapManagers)
            {
                if (mapManager)
                    yield return mapManager.EndGeneration();
            }
        }
    }
}