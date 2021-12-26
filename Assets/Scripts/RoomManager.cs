using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoomManager : MonoBehaviour
{
    public static List<Room> usedRoom;
    public static List<Vector2Int> usedRoomOffset;
    static List<Room> availableRoom;
    public static int[,] roomMap;
    static List<RoomSpawnPoint> roomSpawnPoints;
    //static List<RoomSpawnPoint> mainSpawnPoints;

    public int size = 6;
    public int mapRadius = 3;
    public int trial = 5;
    //public Room[] level1Rooms;

    public void GenerateLevel(string path) {
        //clear
        usedRoom = new List<Room>();
        usedRoomOffset = new List<Vector2Int>();
        availableRoom = new List<Room>();
        roomMap = new int[mapRadius*2+1,mapRadius*2+1]; //center at [r,r]
        roomSpawnPoints = new List<RoomSpawnPoint>();
        //load
        availableRoom = Resources.LoadAll<Room>(path).ToList(); //https://forum.unity.com/threads/how-to-properly-create-an-array-of-all-scriptableobjects-in-a-folder.794109/
        //calculate all signature
        foreach(Room room in availableRoom) {
            room.CalculateSignature();
        }
        Debug.Log("--------------------Creating Room--------------------");
        Debug.Log("Creating level with room pool of " + availableRoom.Count);
        //generate 1
        roomSpawnPoints.Add(new RoomSpawnPoint(mapRadius, mapRadius, RoomSpawnPoint.DoorDir.Start, null));
        if (AddRoomSingle(ref roomSpawnPoints, ref roomSpawnPoints, ref roomMap, ref availableRoom, ref usedRoom, ref usedRoomOffset)) {
            RoomDebugDisplay();
        } else {
            Debug.Log("FATAL ERROR"); // this should NEVER show up unless the room pool is empty
        }
    }

    public void GenerateRoom() {
        Debug.Log("--------------------Generate Room--------------------");
        if (AddRoomSingle(ref roomSpawnPoints, ref roomSpawnPoints, ref roomMap, ref availableRoom, ref usedRoom, ref usedRoomOffset)) {
            RoomDebugDisplay();
        } else {
            Debug.Log("Can't add room within the current number of trial");
        }
    }

    public void RoomDebugDisplay() {
        for (int y=0;y<mapRadius*2+1;y++) {
            string temp = "";
            for (int x=0;x<mapRadius*2+1;x++) {
                temp+=("("+x+","+y+")"+roomMap[x,y]+"\t");
            }
            Debug.Log(temp);
        }
    }

    public void RoomDebugInfo() {
        Debug.Log("usedRoom count: " + usedRoom.Count);
        Debug.Log("usedRoomOffset count: " + usedRoomOffset.Count);
        Debug.Log("availableRoom count: " + availableRoom.Count);
        Debug.Log("roomSpawnPoints count: " + roomSpawnPoints.Count);
    }

    public bool AddRoomSingle(ref List<RoomSpawnPoint> rspsFrom, ref List<RoomSpawnPoint> rspsNew, ref int[,] roomMap, ref List<Room> availableRoom, ref List<Room> usedRoom, ref List<Vector2Int> usedRoomOffset) {
        //random 1 rsp from rspsFrom and add new rsp(s) to rspsNew
        //and add new room

        //declare essentials
        List<RoomSpawnPoint> rspsAttempt = new List<RoomSpawnPoint>(rspsFrom);
        bool rspSuccess = false;
        List<RoomSpawnPoint> targetRsps = new List<RoomSpawnPoint>();
        List<RoomSpawnPoint> outputRsps = new List<RoomSpawnPoint>();
        List<Room> newAvailableRoom = new List<Room>(availableRoom);
        List<Room> newUsedRoom = new List<Room>(usedRoom);
        List<Vector2Int> newUsedRoomOffset = new List<Vector2Int>(usedRoomOffset);

        //start main loop
        for (int attemptRsp=0; attemptRsp<trial; attemptRsp++) {
            Debug.Log("AddRoomSingle attempt:" + attemptRsp);
            //not enough from previous attempt
            if (rspsAttempt.Count <= 0) {
                Debug.Log("ERROR, not sufficient rsps to find compatible room");
                break;
            }

            //random rsps and get filter
            targetRsps = new List<RoomSpawnPoint>();
            targetRsps = RandomRoomSpawnPoint(rspsAttempt);
            string roomFilter = ExtractMatchingRoomFilterFromRsps(targetRsps);
            Debug.Log("\troomFilter: " + roomFilter);

            //list all room with compatible area signature
            List<Room> filteredRooms = RoomListFromRoomFilter(availableRoom, roomFilter);
            if (filteredRooms.Count == 0) {
                rspsAttempt = rspsAttempt.Except<RoomSpawnPoint>(targetRsps).ToList<RoomSpawnPoint>();
                continue;
            }

            //start room loop, iterate through signature filtered room
            int countRoomAttempt = 0;
            while (filteredRooms.Count > 0) {

                //start at random
                Room targetRoom = RandomRoomFromRoomList(filteredRooms);
                Debug.Log("\tRoomAttempt#"+countRoomAttempt+" ["+targetRsps[0].x+","+targetRsps[0].y+"] <- \""+targetRoom.name+"\"\t|roomRsps|: "+targetRoom.roomSpawnPointInfos.Count);

                //add room and check
                //changes will be made to roomMap and outputRsps only if it success
                if (AddRoomCheck(ref roomMap, targetRsps[0], targetRoom, roomFilter, ref outputRsps, ref newUsedRoomOffset, rspsFrom)) {
                    //success
                    rspSuccess = true;
                    newAvailableRoom.Remove(targetRoom);
                    targetRoom.roomAddedNumber = newUsedRoom.Count;
                    newUsedRoom.Add(targetRoom);
                    break;
                } else {
                    filteredRooms.Remove(targetRoom);
                }
                countRoomAttempt++;
            }
            if (rspSuccess) break;
        }
        //abandon changes
        if (!rspSuccess) return false;
        //apply changes
        RemoveRspsOverlapArea(ref rspsFrom, roomMap);
        rspsFrom = rspsFrom.Except<RoomSpawnPoint>(targetRsps).ToList<RoomSpawnPoint>();
        rspsNew = rspsNew.Union<RoomSpawnPoint>(outputRsps).ToList<RoomSpawnPoint>();
        availableRoom = newAvailableRoom;
        usedRoom = newUsedRoom;
        usedRoomOffset = newUsedRoomOffset;
        return true;
    }

    private List<RoomSpawnPoint> RandomRoomSpawnPoint(List<RoomSpawnPoint> roomSpawnPoints) {
        //random for 1 position of rsp but return a list because some rsps may overlap
        if (roomSpawnPoints.Count == 1) return roomSpawnPoints;
        int randomIndex = Random.Range(0, roomSpawnPoints.Count);
        RoomSpawnPoint randomRsp = roomSpawnPoints[randomIndex]; //choose 1 random as a base
        List<RoomSpawnPoint> samePosPoints = new List<RoomSpawnPoint>(); //use to store points with the same location
        foreach(RoomSpawnPoint rsp in roomSpawnPoints) {
            if (randomRsp.x == rsp.x && randomRsp.y == rsp.y) { //same loc or just same element
                samePosPoints.Add(rsp);
            }
        }
        return samePosPoints;
    }

    private string ExtractMatchingRoomFilterFromRsps(List<RoomSpawnPoint> rsps) {
        string roomFilter = ""; //will be added coresponding to the number of connections.
        foreach(RoomSpawnPoint rsp in rsps) {
            switch (rsp.dir) { //based on dir, the connecting room will need to be filtered
                case RoomSpawnPoint.DoorDir.Right: //right door connected to left door and so on
                    roomFilter += "L";
                    break;
                case RoomSpawnPoint.DoorDir.Top:
                    roomFilter += "B";
                    break;
                case RoomSpawnPoint.DoorDir.Left:
                    roomFilter += "R";
                    break;
                case RoomSpawnPoint.DoorDir.Bottom:
                    roomFilter += "T";
                    break;
                case RoomSpawnPoint.DoorDir.Start:
                    roomFilter += "RTLB";
                    break;
            }
        }
        return roomFilter;
    } 

    private List<Room> RoomListFromRoomFilter(List<Room> rooms, string roomFilter) {
        List<Room> filteredRoom = new List<Room>();
        foreach(Room room in rooms) {
            bool matchFilter = false;
            foreach(KeyValuePair<Vector2Int,string> signature in room.signatures) {
                foreach(char chr in roomFilter) {
                    if (signature.Value.Contains(""+chr)) {
                        matchFilter = true;
                        break;
                    }
                }
                if (matchFilter) break;
            }
            if (matchFilter) filteredRoom.Add(room);
        }
        return filteredRoom;
    }

    private Room RandomRoomFromRoomList(List<Room> rooms) {
        if (rooms.Count == 1) return rooms[0];
        int randomIndex = Random.Range(0, rooms.Count);
        return rooms[randomIndex];
    }

    private void RemoveRspsOverlapArea(ref List<RoomSpawnPoint> rsps, int[,] roomMap) {
        List<RoomSpawnPoint> removeList = new List<RoomSpawnPoint>();
        foreach(RoomSpawnPoint rsp in rsps) {
            if (roomMap[rsp.x, rsp.y] >= 10) {
                removeList.Add(rsp);
            }
        }
        rsps = rsps.Except<RoomSpawnPoint>(removeList).ToList<RoomSpawnPoint>();
    }

    private bool AddRoomCheck(ref int[,] roomMap, RoomSpawnPoint posRsp, Room room, string roomFilter, ref List<RoomSpawnPoint> outputRsps, ref List<Vector2Int> usedRoomOffset, List<RoomSpawnPoint> rspsFrom) {
        //make a copy that will be apply when success
        int[,] newRoomMap = new int[roomMap.GetLength(0),roomMap.GetLength(1)];
        System.Array.Copy(roomMap, newRoomMap, roomMap.GetLength(0)*roomMap.GetLength(1));
        
        //count connections
        int connections = 0;
        
        //get room pos pool with matching with compatible signature
        List<Vector2Int> posPool = PosPoolFromRoomFilter(room.signatures, roomFilter);
        Debug.Log("\t\t|room.signatures|: "+room.signatures.Count);
        Debug.Log("\t\t|posPool|: "+posPool.Count);

        //Loop through all possible pos
        bool addRoomSuccess = true;
        int countPosAttempt = 0;
        while (posPool.Count > 0) {
            int randomIndex = Random.Range(0, posPool.Count);
            Vector2Int doorPos = posPool[randomIndex];
            
            //calculate offset that if sum into centric coordinate of room area will get its pos on map
            Vector2Int offset = new Vector2Int(posRsp.x-doorPos.x, posRsp.y-doorPos.y);
            room.onMapOffset = offset; //will be used
            Debug.Log("\t\tPosAttempt#"+countPosAttempt+" offset: ("+offset.x+","+offset.y+")");
            List<Vector2Int> overlappedRspPos = new List<Vector2Int>();//save interesting overlapped/non-primary rsp position, it will not be inspected if add room fail
            countPosAttempt++;
            //for each roomAreas, check if occupied on roomMap
            foreach(Vector2Int roomArea in room.roomAreas) {
                int mapX = roomArea.x+offset.x;
                int mapY = roomArea.y+offset.y;
                if (newRoomMap[mapX, mapY] >= 10) {
                    //overlapped
                    addRoomSuccess = false;
                    break;
                } else if (newRoomMap[mapX, mapY] == 0) {
                    newRoomMap[mapX, mapY] = 11;
                } else {
                    newRoomMap[mapX, mapY] = 11;
                    //found non-primary rsp, save for possible connection
                    overlappedRspPos.Add(new Vector2Int(mapX, mapY));
                }
            }
            if (!addRoomSuccess) {//try with new posPool
                //remove all with value doorPos and continue
                posPool.RemoveAll(pos => pos.Equals(doorPos));
                continue;
            }
            string debugRoomStatement = "\t\t\tSuccess, outputRsps: ";
            //foreach in room.roomSpawnPointInfos, but key so that I can change value
            List<Vector2Int> rspInfoPosList = new List<Vector2Int>(room.roomSpawnPointInfos.Keys);
            foreach(Vector2Int rspInfoPos in rspInfoPosList) {
                //will need to apply change after
                RoomSpawnPointInfo rspInfoValue = room.roomSpawnPointInfos[rspInfoPos];
                int mapX = rspInfoPos.x+offset.x;//rsp pos on roomMap
                int mapY = rspInfoPos.y+offset.y;
                debugRoomStatement += "\t"+rspInfoValue.direction+"("+mapX+","+mapY+")";

                //assigning possible new rsp
                if (newRoomMap[mapX, mapY] >= 10) {
                    //If new room rsp overlap a room, and the other room rsp also overlap in opposite direction, the connection is created
                    //foreach pos of rsp in room, iterate through rspsFrom and look for connection
                    
                    //get room that rsp stand on
                    Vector2Int rspRoomPos = GetRspRoomPosFromDirection(new Vector2Int(mapX,mapY), rspInfoValue.direction);

                    //Check if rspRoomPos overlapped with EXISTING rsp from rspsFrom
                    foreach (RoomSpawnPoint otherRsp in rspsFrom) {
                        //check 2 thigs
                        //check for the pos of otherRsp
                        //Debug.Log(rspRoomPos.x+","+rspRoomPos.y+"|"+otherRsp.x+","+otherRsp.y);
                        if (!(rspRoomPos.x == otherRsp.x && rspRoomPos.y == otherRsp.y)) continue;
                        //check for opposite direction
                        if (!(rspInfoValue.direction == "R" && otherRsp.dir == RoomSpawnPoint.DoorDir.Left ||
                        rspInfoValue.direction == "T" && otherRsp.dir == RoomSpawnPoint.DoorDir.Bottom ||
                        rspInfoValue.direction == "L" && otherRsp.dir == RoomSpawnPoint.DoorDir.Right ||
                        rspInfoValue.direction == "B" && otherRsp.dir == RoomSpawnPoint.DoorDir.Top)) {
                            continue;
                        }
                        //CONNECT
                        connections++;
                        Vector2Int otherRspInfoPos = new Vector2Int(otherRsp.x-otherRsp.room.onMapOffset.x,otherRsp.y-otherRsp.room.onMapOffset.y);
                        RoomSpawnPointInfo otherRspInfoValue = otherRsp.room.roomSpawnPointInfos[otherRspInfoPos];
                        EstablishConnection(room, ref rspInfoValue, rspInfoPos, otherRsp.room, ref otherRspInfoValue, otherRspInfoPos);
                        //apply changes
                        otherRsp.room.roomSpawnPointInfos[otherRspInfoPos] = otherRspInfoValue;
                        room.roomSpawnPointInfos[rspInfoPos] = rspInfoValue;
                        break;
                    }
                    
                    //apply changes
                } else { //0,1,2,3,4
                    RoomSpawnPoint.DoorDir dir;
                    switch (rspInfoValue.direction) {
                        case "R":
                            dir = RoomSpawnPoint.DoorDir.Right;
                            newRoomMap[mapX, mapY] = 1;
                            break;
                        case "T":
                            dir = RoomSpawnPoint.DoorDir.Top;
                            newRoomMap[mapX, mapY] = 2;
                            break;
                        case "L":
                            dir = RoomSpawnPoint.DoorDir.Left;
                            newRoomMap[mapX, mapY] = 3;
                            break;
                        default:
                            dir = RoomSpawnPoint.DoorDir.Bottom;
                            newRoomMap[mapX, mapY] = 4;
                            break;
                    }
                    outputRsps.Add(new RoomSpawnPoint(mapX, mapY, dir, room));
                }
            }
            Debug.Log(debugRoomStatement);
            usedRoomOffset.Add(offset);
            break;
        }
        if (!addRoomSuccess) return false;

        //if success
        roomMap = newRoomMap;
        Debug.Log(connections + " connections created");
        return true;
    }

    private List<Vector2Int> PosPoolFromRoomFilter(Dictionary<Vector2Int,string> signatures, string roomFilter) {
        //the more it match, the higher chance it had to be pick
        List<Vector2Int> posPool = new List<Vector2Int> ();
        foreach(KeyValuePair<Vector2Int,string> signature in signatures) {
            foreach(char chr in roomFilter) {
                if (signature.Value.Contains(""+chr)) {
                    posPool.Add(signature.Key);
                    //Debug.Log(signature.Value + "-" + chr + "=" + roomFilter);
                }
            }
        }
        return posPool;
    }

    private Vector2Int GetRspRoomPosFromDirection(Vector2Int pos, string dir) {
        Vector2Int posOffset = Vector2Int.zero;
        switch (dir) {
            case "R":
                posOffset.x -= 1;
                break;
            case "T":
                posOffset.y += 1;
                break;
            case "L":
                posOffset.x += 1;
                break;
            default: //default will only include case for "B" and not start, because, start can never overlap with another
                posOffset.y -= 1;
                break;
        } 
        return new Vector2Int(pos.x + posOffset.x, pos.y + posOffset.y);
    }

    private void EstablishConnection(Room room1, ref RoomSpawnPointInfo rspInfo1, Vector2Int rspCentric1, Room room2, ref RoomSpawnPointInfo rspInfo2, Vector2Int rspCentric2) {
        //rspInfo
        rspInfo1.destinationId = rspInfo2.sourceId;
        rspInfo1.destinationRspPos = rspCentric2;
        rspInfo1.destinationRoom = room2;
        room1.debugRoomSpawnPointDestinations += "("+rspInfo2.sourceId+"|"+rspInfo1.direction+")";

        rspInfo2.destinationId = rspInfo1.sourceId;
        rspInfo2.destinationRspPos = rspCentric1;
        rspInfo2.destinationRoom = room1;
        room2.debugRoomSpawnPointDestinations += "("+rspInfo1.sourceId+"|"+rspInfo2.direction+")";
    }

    public struct RoomSpawnPoint {
        public int x;
        public int y;
        public enum DoorDir {Right,Top,Left,Bottom,Start};
        public DoorDir dir;
        public Room room;
        
        public RoomSpawnPoint(int x, int y, DoorDir dir, Room room) {
            this.x = x;
            this.y = y;
            this.dir = dir;
            this.room = room;
        }
    }
}
