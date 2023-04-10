using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace redd096
{
    #region editor

#if UNITY_EDITOR

    using UnityEditor;

    [CustomEditor(typeof(ProceduralMapManager), true)]
    [CanEditMultipleObjects]
    public class ProceduralMapManagerEditor : Editor
    {
        private ProceduralMapManager mapManager;

        private void OnEnable()
        {
            mapManager = target as ProceduralMapManager;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);

            if (GUILayout.Button("Destroy Map"))
            {
                mapManager.DestroyMap();

                //set undo
                Undo.RegisterFullObjectHierarchyUndo(target, "Regen World");
            }
        }
    }

#endif

    #endregion

    //scriptable object class
    public abstract class ProceduralMapManagerCheck : ScriptableObject
    {
        public abstract bool IsValid(Room roomToPlace, ProceduralMapManager mapManager);
    }

    [System.Serializable]
    public class StructFixedRooms
    {
        [Min(0)] public int minID;
        [Min(0)] public int maxID;
        public Room[] roomPrefabs;
    }

    [System.Serializable]
    public class StructFillerRooms
    {
        public string nameList;
        [Range(0, 100)] public int percentage = 100;
        public Room[] roomPrefabs;
    }

    [AddComponentMenu("redd096/Procedural Map/Procedural Map Manager")]
    public class ProceduralMapManager : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] bool regenOnPlay = true;
        [Min(1)] [SerializeField] int numberRooms = 12;
        [SerializeField] protected ProceduralMapManagerCheck[] checks = default;

        [Header("Attempts")]
        [Min(1)] [SerializeField] int maxAttempts = 5;
        [Min(1)] [SerializeField] int roomsPerAttempt = 5;
        [Min(1)] [SerializeField] int doorsPerAttempt = 2;

        [Header("Filler Rooms - be sure the sum of every percentage is 100")]
        [SerializeField] protected StructFillerRooms[] fillerRooms = default;

        [Header("Fixed Rooms")]
        [SerializeField] protected StructFixedRooms[] fixedRooms = default;

        //rooms
        List<Room> rooms = new List<Room>();
        private int roomID;
        private Room lastRoom;
        private bool succeded;
        private bool teleported;

        //for fixed rooms
        List<StructFixedRooms> fixedRoomsAlreadyChecked = new List<StructFixedRooms>();
        StructFixedRooms fixedRoomTryingToPut;
        List<StructFixedRooms> fixedRoomsAlreadyPut = new List<StructFixedRooms>();

        //to use more map managers
        public List<Room> Rooms => rooms;
        public List<Room> RoomsEveryOtherMapManager { get; private set; } = new List<Room>();

        void Start()
        {
            //regen map at start
            if (regenOnPlay)
            {
                DestroyMap();
                StartCoroutine(CreateMap());
            }
        }

        #region public API

        public void DestroyMap()
        {
            //clear rooms
            rooms.Clear();
            roomID = 0;
            lastRoom = null;

            //clear fixed rooms
            fixedRoomsAlreadyChecked.Clear();
            fixedRoomTryingToPut = null;
            fixedRoomsAlreadyPut.Clear();

            //remove every child
            foreach (Transform child in transform)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    EditorApplication.delayCall += () => DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
        }

        public IEnumerator CreateMap(List<Room> roomsEveryOtherMapManager = null)
        {
            //save others map manager rooms
            if (roomsEveryOtherMapManager != null)
            {
                this.RoomsEveryOtherMapManager = roomsEveryOtherMapManager;
                //roomID = roomsEveryOtherMapManager.Count;
            }

            int attempts = 0;

            while (rooms.Count < numberRooms && attempts < maxAttempts)
            {
                //get room prefab, if can't, destroy dungeon and recreate (failed to put some room)
                Room newRoomPrefab = GetRoomPrefab();
                if (newRoomPrefab == null)
                    break;

                //generate room
                Room newRoom = Instantiate(newRoomPrefab, transform);

                //try to positionate
                yield return PositionRoom(newRoom);

                //if succeded, register it
                if (succeded)
                {
                    RegisterRoom(newRoom);
                    attempts = 0;
                }
                //else, destroy and try new one
                else
                {
                    Destroy(newRoom.gameObject);
                    Debug.Log("<color=orange>Shitty room, make another one</color>");
                    attempts++;
                }
            }

            //if not reach number rooms, regen
            if (rooms.Count < numberRooms)
            {
                //destroy old and create new one
                Debug.Log("<color=red>Shitty map, cry and restart</color>");
                DestroyMap();
                yield return CreateMap(roomsEveryOtherMapManager);
            }
            else
            {
                //if unique map manager, end generation
                if (regenOnPlay)
                    yield return EndGeneration();

                Debug.Log("<color=cyan>Mission complete!</color>");
            }
        }

        public virtual IEnumerator EndGeneration()
        {
            //foreach room created, call function
            foreach (Room room in rooms)
            {
                yield return room.EndRoom();
            }
        }

        #endregion

        #region private API

        Room GetRoomPrefab()
        {
            //quando istanzi, controlla se ci sono stanze fisse
            //se non si attacca, vedere se ce ne sono altre
            //se una stanza non viene creata nel suo range, distruggere e ricreare il dungeon

            //quando istanzi, controlla se ci sono stanze fisse
            //aggiungere alla lista delle già checkate, così se non si attacca ne viene provata un'altra
            //quando viene posizionata una stanza pulisci le già checkate per riprovarle tutte, ma se quella creata era una di quelle fisse aggiungila alla lista già piazzate per non riprovarla
            //se la stanza fissa non viene posizionata ma ha il range più alto, si può riprovare al posizionamento successivo, ma assicurarsi di resettare la variabile se si posiziona una stanza non fissa
            //se una stanza non viene creata nel suo range, distruggere e ricreare il dungeon

            //if a room failed at its maxID, recreate dungeon
            if (fixedRoomTryingToPut != null && roomID >= fixedRoomTryingToPut.maxID)
            {
                return null;
            }

            //check fixed rooms
            foreach (StructFixedRooms checkRoom in fixedRooms)
            {
                //be sure is not already checked
                if (fixedRoomsAlreadyChecked.Contains(checkRoom) || fixedRoomsAlreadyPut.Contains(checkRoom))
                {
                    continue;
                }

                //if there is one who need this ID
                if (roomID >= checkRoom.minID && roomID <= checkRoom.maxID)
                {
                    //add to list already check
                    fixedRoomsAlreadyChecked.Add(checkRoom);

                    //return its prefab
                    fixedRoomTryingToPut = checkRoom;
                    return checkRoom.roomPrefabs[Random.Range(0, checkRoom.roomPrefabs.Length)];
                }
            }

            //else check for filler room
            fixedRoomTryingToPut = null;

            //get random value from 1 to 100
            int random = Mathf.Max(1, Mathf.RoundToInt(Random.value * 100));
            int currentPercentage = 0;

            //foreach filler, check if percentage is inside random value
            foreach (StructFillerRooms filler in fillerRooms)
            {
                //if inside, return its prefab
                if (filler.percentage + currentPercentage >= random)
                    return filler.roomPrefabs[Random.Range(0, filler.roomPrefabs.Length)];

                //else, add to currentPercentage, to check next one
                currentPercentage += filler.percentage;
            }

            return null;
        }

        private IEnumerator PositionRoom(Room newRoom)
        {
            succeded = false;
            teleported = false;

            if (rooms.Count <= 0 && RoomsEveryOtherMapManager.Count <= 0)
            {
                //first room
                Debug.Log("<color=lime>positioned first room</color>");
                newRoom.SetPosition(Vector3.zero);
                succeded = true;
                yield break;
            }
            else
            {
                //attach to first room of another map manager
                if (rooms.Count <= 0)
                    lastRoom = RoomsEveryOtherMapManager[0];
                //else try attach to last room
                else
                    lastRoom = rooms[rooms.Count - 1];

                for (int roomLoop = 0; roomLoop < roomsPerAttempt; roomLoop++)
                {
                    for (int doorLoop = 0; doorLoop < doorsPerAttempt; doorLoop++)
                    {
                        //try attach to door
                        if (newRoom.SetPosition(lastRoom.GetRandomDoor(), lastRoom, this) && IsValidPlace(newRoom))
                        {
                            Debug.Log("<color=lime>positioned room " + roomID + "</color>");
                            succeded = true;
                            yield break;
                        }

                        //else try another door
                        yield return null;
                    }

                    //else try another room
                    Debug.Log("<color=yellow>changed room</color>");
                    teleported = true;                                      //set teleported true
                    lastRoom = rooms.Count > 0 ? rooms[Random.Range(0, rooms.Count - 1)] : RoomsEveryOtherMapManager[0];   //if rooms at 0, try again first room of another map manager
                }
            }
        }

        bool IsValidPlace(Room roomToPlace)
        {
            //foreach check, be sure is valid
            foreach (ProceduralMapManagerCheck check in checks)
            {
                if (check && !check.IsValid(roomToPlace, this))
                    return false;
            }

            return true;
        }

        private void RegisterRoom(Room newRoom)
        {
            //add to list and update ID
            rooms.Add(newRoom);
            newRoom.Register(roomID, teleported);
            roomID++;

            //update fixed rooms
            UpdateFixedRooms();
        }

        void UpdateFixedRooms()
        {
            //clear rooms already checked 
            fixedRoomsAlreadyChecked.Clear();

            //and add to list already put
            if (fixedRoomTryingToPut != null)
            {
                fixedRoomsAlreadyPut.Add(fixedRoomTryingToPut);
                fixedRoomTryingToPut = null;
            }
        }

        #endregion
    }
}