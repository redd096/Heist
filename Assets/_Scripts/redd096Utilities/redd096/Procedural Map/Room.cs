using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace redd096
{
    public enum TypeOfDoor
    {
        both, onlyEnter, onlyExit
    }

    public enum CardinalDirection
    {
        up, down, left, right
    }

    [System.Serializable]
    public struct DoorStruct
    {
        public Transform doorTransform;
        public CardinalDirection direction;
        public TypeOfDoor typeOfDoor;
    }

    [AddComponentMenu("redd096/Procedural Map/Room")]
    [SelectionBase]
    public class Room : MonoBehaviour
    {
        #region variables

        //TODO use doors on every room to instantiate. No limit Y or Z
        [Header("Use Z instead of Y")]
        [SerializeField] bool useZ = true;

        [Header("Important")]
        [Tooltip("Size of every tile which compose this room")] [SerializeField] float tileSize = 1f;
        [Tooltip("Int because the size will be exspressed in tiles")] [SerializeField] int width = 1;
        [Tooltip("Int because the size will be exspressed in tiles")] [SerializeField] int height = 1;
        [SerializeField] protected List<DoorStruct> doors = new List<DoorStruct>();

        [Header("DEBUG")]
        [SerializeField] bool drawRoomArea = true;
        [SerializeField] Color colorRoomArea = Color.cyan;

        float HalfWidth => width * tileSize * 0.5f;
        float HalfHeight => height * tileSize * 0.5f;
        Vector2 UpRight => useZ ? new Vector3(transform.position.x + HalfWidth, transform.position.z + HalfHeight) : new Vector3(transform.position.x + HalfWidth, transform.position.y + HalfHeight);
        Vector2 DownLeft => useZ ? new Vector3(transform.position.x - HalfWidth, transform.position.z - HalfHeight) : new Vector3(transform.position.x - HalfWidth, transform.position.y - HalfHeight);

        protected int id = 0;
        protected bool teleported;

        DoorStruct adjacentDoor = default;
        Room adjacentRoom = default;
        DoorStruct entranceDoor = default;
        protected List<DoorStruct> usedDoors = new List<DoorStruct>();

        #endregion

        protected virtual void OnDrawGizmos()
        {
            if (drawRoomArea)
            {
                Gizmos.color = colorRoomArea;

                //draw room size
                Gizmos.DrawWireCube(transform.position, new Vector3(width * tileSize, useZ ? 0 : height * tileSize, useZ ? height * tileSize : 0));

                Gizmos.color = Color.white;
            }
        }

        #region public API

        public void SetPosition(Vector3 position)
        {
            //set position
            transform.position = position;
        }

        public bool SetPosition(DoorStruct adjacentDoor, Room adjacentRoom, ProceduralMapManager mapManager)
        {
            //set references
            this.adjacentDoor = adjacentDoor;
            this.adjacentRoom = adjacentRoom;

            if (adjacentRoom == null || adjacentDoor.doorTransform == null)
                Debug.Log("<color=red>Houston, abbiamo un problema</color>");

            //check this room has a door to adjacent room, and adjust position
            if (CheckDoors() == false)
                return false;

            //check this room is not inside another room and viceversa
            if (CheckOverlap(mapManager) == false)
                return false;

            return true;
        }

        public DoorStruct GetRandomDoor()
        {
            List<DoorStruct> possibleDoors = new List<DoorStruct>();

            //add every door except only enter
            foreach (DoorStruct doorStruct in doors)
            {
                if (usedDoors.Contains(doorStruct))                     //don't check already used doors
                    continue;

                if (doorStruct.typeOfDoor != TypeOfDoor.onlyEnter)      //be sure is not OnlyEnter, because this one will be an exit from adjacent room
                    possibleDoors.Add(doorStruct);
            }

            //give a door, just to fail at position room
            if (possibleDoors.Count <= 0)
                return doors[0];

            //return random door
            return possibleDoors[Random.Range(0, possibleDoors.Count)];
        }

        public virtual void Register(int id, bool teleported)
        {
            this.id = id;
            this.teleported = teleported;

            //set our entrance and adjacentRoom's exit doors used
            if (adjacentRoom)
            {
                usedDoors.Add(entranceDoor);
                adjacentRoom.usedDoors.Add(adjacentDoor);
            }
        }

        public virtual IEnumerator EndRoom()
        {
            yield return null;
        }

        #endregion

        #region private API

        bool CheckDoors()
        {
            //if moving left, check doors on right, else check doors on left
            if (adjacentDoor.direction == CardinalDirection.left || adjacentDoor.direction == CardinalDirection.right)
            {
                entranceDoor.direction = adjacentDoor.direction == CardinalDirection.left ? CardinalDirection.right : CardinalDirection.left;
            }
            //if moving up, check doors on bottom, else check doors on top
            else
            {
                entranceDoor.direction = adjacentDoor.direction == CardinalDirection.up ? CardinalDirection.down : CardinalDirection.up;
            }

            List<DoorStruct> possibleDoors = new List<DoorStruct>();

            //add every possible door (using direction setted before)
            foreach (DoorStruct possibleDoor in doors)
            {
                if (possibleDoor.direction == entranceDoor.direction
                    && possibleDoor.typeOfDoor != TypeOfDoor.onlyExit)                //be sure is not OnlyExit, because this one will be an entrance to this room
                {
                    possibleDoors.Add(possibleDoor);
                }
            }

            //if no possible doors, return
            if (possibleDoors.Count <= 0)
                return false;

            //else get a random door between possibles
            entranceDoor = possibleDoors[Random.Range(0, possibleDoors.Count)];

            //calculate distance and move
            Vector3 fromDoorToAdjacentDoor = adjacentDoor.doorTransform.position - entranceDoor.doorTransform.position;
            transform.position += fromDoorToAdjacentDoor;

            return true;
        }

        bool CheckOverlap(ProceduralMapManager mapManager)
        {
            //check rooms of this map manager and also rooms of others map managers
            List<Room> roomsToCheck = new List<Room>(mapManager.Rooms);
            if (mapManager.RoomsEveryOtherMapManager != null)
                roomsToCheck.AddRange(mapManager.RoomsEveryOtherMapManager);

            //foreach room
            foreach (Room room in roomsToCheck)
            {
                //check if there is a room inside this one
                if (PointsInsideRoom(room))
                    return false;

                //check if this one is inside a room
                if (room.PointsInsideRoom(this))
                    return false;
            }

            return true;
        }

        bool PointsInsideRoom(Room roomToCheck)
        {
            //get every tile from downleft (<= so check also DownRight, UpLeft, UpRight)
            for (int x = 0; x <= roomToCheck.width; x++)
            {
                for (int y = 0; y <= roomToCheck.height; y++)
                {
                    //direction right up, if reached limit go backward
                    Vector2 directionGap = new Vector2(x >= roomToCheck.width ? -1 : 1, y >= roomToCheck.height ? -1 : 1);

                    Vector2 point = roomToCheck.DownLeft + (Vector2.right * x * roomToCheck.tileSize) + (Vector2.up * y * roomToCheck.tileSize)     //down left of every tile
                        + (directionGap * roomToCheck.tileSize * 0.1f);                                                                             //little gap to no have half room inside another

                    //check is inside this room
                    if (point.x > DownLeft.x && point.x < UpRight.x)
                    {
                        if (point.y > DownLeft.y && point.y < UpRight.y)
                        {
                            return true;
                        }
                    }
                }
            }

            //check also center if inside this room
            Vector2 center = useZ ? new Vector2(roomToCheck.transform.position.x, roomToCheck.transform.position.z) : new Vector2(roomToCheck.transform.position.x, roomToCheck.transform.position.y);
            if (center.x > DownLeft.x && center.x < UpRight.x)
            {
                if (center.y > DownLeft.y && center.y < UpRight.y)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}