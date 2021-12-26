using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Room", menuName = "ScriptableObjects/Room")]
public class Room : ScriptableObject
{
    public string roomId;
    public RoomComponent roomComponent;
    [ReadOnly] public Dictionary<Vector2Int,string> signatures = new Dictionary<Vector2Int, string>();
    [ReadOnly] public List<Vector2Int> roomAreas = new List<Vector2Int>();
    [ReadOnly] public Dictionary<Vector2Int,RoomSpawnPointInfo> roomSpawnPointInfos = new Dictionary<Vector2Int, RoomSpawnPointInfo>();

    [ReadOnly] public int roomAddedNumber = -1;
    [ReadOnly] public Vector2Int onMapOffset = new Vector2Int();
    [ReadOnly] public string debugRoomSpawnPointDestinations = "";

    public void CalculateSignature() {
        signatures = new Dictionary<Vector2Int, string>();
        roomAreas = new List<Vector2Int>();
        roomSpawnPointInfos = new Dictionary<Vector2Int, RoomSpawnPointInfo>();
        roomAddedNumber = -1;
        onMapOffset = Vector2Int.zero;
        debugRoomSpawnPointDestinations = "";

        RoomComponent.Component[,] componentCell = roomComponent.GetCell2d();
        for (int y=0;y<componentCell.GetLength(1);y++) {
            for (int x=0;x<componentCell.GetLength(0);x++) {
                RoomComponent.Component target = componentCell[x,y];
                if (target == RoomComponent.Component.No) continue; //blank space

                //centralized
                Vector2Int centricPos = new Vector2Int(x-RoomComponent.radius, y-RoomComponent.radius);
                //assign to either area or spawn point
                if (target == RoomComponent.Component.Yes) roomAreas.Add(centricPos);
                else {
                    switch (target) {
                        case RoomComponent.Component.Right:
                            roomSpawnPointInfos.Add(centricPos, new RoomSpawnPointInfo("R", roomId, ""));
                            break;
                        case RoomComponent.Component.Top:
                            roomSpawnPointInfos.Add(centricPos, new RoomSpawnPointInfo("T", roomId, ""));
                            break;
                        case RoomComponent.Component.Left:
                            roomSpawnPointInfos.Add(centricPos, new RoomSpawnPointInfo("L", roomId, ""));
                            break;
                        case RoomComponent.Component.Bottom:
                            roomSpawnPointInfos.Add(centricPos, new RoomSpawnPointInfo("B", roomId, ""));
                            break;
                    }
                    continue;
                }

                //area signature assigning
                if (target == RoomComponent.Component.Yes) {
                    string temp = "";
                    //check for adjacent door
                    if (componentCell[x+1,y] == RoomComponent.Component.Right) temp += "R";
                    if (componentCell[x,y-1] == RoomComponent.Component.Top) temp += "T";
                    if (componentCell[x-1,y] == RoomComponent.Component.Left) temp += "L";
                    if (componentCell[x,y+1] == RoomComponent.Component.Bottom) temp += "B";
                    
                    signatures.Add(centricPos, temp);
                }
            }
        }
    }
}
public struct RoomSpawnPointInfo {
    public string direction;
    public string sourceId;
    public string destinationId;
    public Vector2Int destinationRspPos;
    public Room destinationRoom;

    public RoomSpawnPointInfo(string direction, string sourceId, string destinationId) {
        this.direction = direction;
        this.sourceId = sourceId;
        this.destinationId = destinationId;
        this.destinationRspPos = Vector2Int.zero;
        this.destinationRoom = null;
    }

    public void Reset() {
        direction = "";
        sourceId = "";
        destinationId = "";
        destinationRspPos = Vector2Int.zero;
    }

}