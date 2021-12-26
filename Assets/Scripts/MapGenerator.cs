using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public void Generate1PixelMap() {
        //generate
        int width = RoomManager.roomMap.GetLength(0);
        int height = RoomManager.roomMap.GetLength(1);
        int[,] roomMap = new int[width,height];
        System.Array.Copy(RoomManager.roomMap, roomMap, width*height);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for (int y=0;y<height;y++) {
            for (int x=0;x<width;x++) {
                if (roomMap[x,y] > 10) {
                    colorMap[y*width+x] = Color.white;
                    continue;
                }
                switch(roomMap[x,y]) {
                    case 0:
                        colorMap[y*width+x] = Color.black;
                        break;
                    case 1:
                        colorMap[y*width+x] = Color.blue;
                        break;
                    case 2:
                        colorMap[y*width+x] = Color.red;
                        break;
                    case 3:
                        colorMap[y*width+x] = Color.yellow;
                        break;
                    case 4:
                        colorMap[y*width+x] = Color.green;
                        break;
                }
            }
        }
        //send output
        MapDisplay display = GetComponent<MapDisplay>();
        display.DrawPixelMap(width, height, colorMap);
    }

    public void GeneratePixelMap(int roomRadius = 3, int doorRadius = 1) {
        //declare essential
        List<Room> usedRoom = RoomManager.usedRoom;
        List<Vector2Int> usedRoomOffset = RoomManager.usedRoomOffset;
        int width = RoomManager.roomMap.GetLength(0);
        int height = RoomManager.roomMap.GetLength(1);
        int scale = roomRadius * 2 + 1;
        int scaledWidth = width * scale;
        int scaledHeight = height * scale;
        Debug.Log("Generating Pixel Map of radius: " + roomRadius + " width: " + width + "->" + scaledWidth);

        //color Map room layer
        Color[] colorMap = new Color[scaledHeight * scaledWidth];

        //draw each usedRoom
        for(int i=0; i<usedRoom.Count; i++) {
            //selection list contains room area and add offset
            HashSet<Vector2Int> selectPosSet = new HashSet<Vector2Int>(usedRoom[i].roomAreas);
            selectPosSet = Vector2IntSetAddOffset(selectPosSet, usedRoomOffset[i]);
            Debug.Log("\tcheck1 |selectPosSet|: " + selectPosSet.Count);
            
            //outline
            HashSet<Vector2Int> selectOutlineSet = Vector2IntSetExpand1(selectPosSet);
            selectOutlineSet.ExceptWith(selectPosSet); //expand exclude
            selectOutlineSet = Vector2IntSetScaleExpand1(selectOutlineSet, roomRadius);
            Debug.Log("\tcheck2 |selectOutlineSet|: " + selectOutlineSet.Count);
            selectPosSet = Vector2IntScale(selectPosSet, roomRadius);
            Debug.Log("\tcheck3 |selectPosSet|: " + selectPosSet.Count);
            selectOutlineSet.IntersectWith(selectPosSet); //doesn't works, probably due to inside being ref class? NOPE i'm an idiot
            //selectOutlineSet = new HashSet<Vector2Int>(selectOutlineSet.Intersect<Vector2Int>(selectPosSet).ToList<Vector2Int>());
            //selectOutlineSet = Vector2IntSetIntersectSet(selectOutlineSet, selectPosSet);
            Debug.Log("\tcheck4 |selectOutlineSet|: " + selectOutlineSet.Count);

            //remove door section from room selection
            //selectOutlineSet = AddDoorSection(selectOutlineSet, usedRoom[i], usedRoomOffset[i], roomRadius, doorRadius);

            //insert select outline set to color map
            Vector2IntSetAddToColorMap(scaledWidth, scaledHeight, selectOutlineSet, ref colorMap);
        }

        //draw
        MapDisplay display = GetComponent<MapDisplay>();
        display.DrawPixelMap(scaledWidth, scaledHeight, colorMap);
    }

    HashSet<Vector2Int> Vector2IntSetAddOffset(HashSet<Vector2Int> posSet, Vector2Int offset) {
        //add offset to match roomMap
        HashSet<Vector2Int> outputSet = new HashSet<Vector2Int>();
        foreach(Vector2Int pos in posSet) {
            Vector2Int temp = new Vector2Int(pos.x, pos.y);
            temp.x += offset.x;
            temp.y += offset.y;
            outputSet.Add(temp);
        }
        return outputSet;
    }

    HashSet<Vector2Int> Vector2IntSetScaleExpand1(HashSet<Vector2Int> posSet, int scaleRadius) {
        //scale and expand and exclude uncleanly but fast
        int scale = scaleRadius * 2 + 1;
        HashSet<Vector2Int> outputSet = new HashSet<Vector2Int>();
        foreach(Vector2Int pos in posSet) {
            int x = pos.x * scale;
            int y = pos.y * scale;
            for (int i=0; i<scale+2; i++) {//right
                outputSet.Add(new Vector2Int(x+scale, y+i-1));
            }
            for (int i=0; i<scale+2; i++) {//top
                outputSet.Add(new Vector2Int(x+i-1, y-1));
            }
            for (int i=0; i<scale+2; i++) {//left
                outputSet.Add(new Vector2Int(x-1, y+i-1));
            }
            for (int i=0; i<scale+2; i++) {//bottom
                outputSet.Add(new Vector2Int(x+i-1, y+scale));
            }
        }
        return outputSet;
    }

    HashSet<Vector2Int> Vector2IntSetExpand1(HashSet<Vector2Int> posSet) {
        //expand but doesn't care inside
        HashSet<Vector2Int> outputSet = new HashSet<Vector2Int>();
        foreach(Vector2Int pos in posSet) {
            outputSet.Add(new Vector2Int(pos.x+1, pos.y));
            outputSet.Add(new Vector2Int(pos.x, pos.y-1));
            outputSet.Add(new Vector2Int(pos.x-1, pos.y));
            outputSet.Add(new Vector2Int(pos.x, pos.y+1));
        }
        return outputSet;
    }

    HashSet<Vector2Int> Vector2IntScale(HashSet<Vector2Int> posSet, int scaleRadius) {
        //just scale it up, with scale radius of 2, what normally took 1 selection becomes 25
        int scale = scaleRadius * 2 + 1;
        HashSet<Vector2Int> outputSet = new HashSet<Vector2Int>();
        foreach(Vector2Int pos in posSet) {
            for (int x=0; x<scale; x++) {
                for (int y=0; y<scale; y++) {
                    outputSet.Add(new Vector2Int(pos.x*scale + x, pos.y*scale + y));
                }
            }
        }
        return outputSet;
    } 

    HashSet<Vector2Int> Vector2IntSetIntersectSet(HashSet<Vector2Int> posSet1, HashSet<Vector2Int> posSet2) {
        HashSet<Vector2Int> outputSet = new HashSet<Vector2Int>();
        foreach(Vector2Int pos1 in posSet1) {
            foreach(Vector2Int pos2 in posSet2) {
                if (pos1.Equals(pos2)) {
                    outputSet.Add(pos1);
                    break;
                }
            }
        }
        return outputSet;
    }

    HashSet<Vector2Int> AddDoorSection(HashSet<Vector2Int> posSet, Room usedRoom, Vector2Int usedRoomOffset, int roomRadius, int doorRadius) {
        HashSet<Vector2Int> newPosSet = new HashSet<Vector2Int>(posSet);
        HashSet<Vector2Int> selectDoorPos = new HashSet<Vector2Int>();

        foreach(KeyValuePair<Vector2Int,RoomSpawnPointInfo> rsp in usedRoom.roomSpawnPointInfos) {
            if (rsp.Value.destinationId == "") continue;
            Vector2Int doorPos = new Vector2Int((rsp.Key.x + usedRoomOffset.x)*(roomRadius*2+1), (rsp.Key.y + usedRoomOffset.y)*(roomRadius*2+1));
            //add to selectDoorPos according to it's direction
            for (int i=0; i<doorRadius*2+1; i++) {
                switch(rsp.Value.direction) {
                    case "R":
                        selectDoorPos.Add(new Vector2Int(doorPos.x-1, doorPos.y+(roomRadius-doorRadius)+i));
                        break;
                    case "T":
                        selectDoorPos.Add(new Vector2Int(doorPos.x+(roomRadius-doorRadius)+i, doorPos.y+2*roomRadius+1));
                        break;
                    case "L":
                        selectDoorPos.Add(new Vector2Int(doorPos.x+2*roomRadius+1, doorPos.y+(roomRadius-doorRadius)+i));
                        break;
                    case "B":
                        selectDoorPos.Add(new Vector2Int(doorPos.x+(roomRadius-doorRadius)+i, doorPos.y-1));
                        break;
                    
                }
            }
        }

        newPosSet.ExceptWith(selectDoorPos);
        //newPosSet.UnionWith(selectDoorPos);
        Debug.Log("select: " + selectDoorPos.Count);
        return newPosSet;
    }

    Color[] Vector2IntSetToColorMap(int width, int height, HashSet<Vector2Int> posSet) {
        Color[] colorMap = new Color[width * height];
        for (int y=0; y<height; y++) {
            for (int x=0; x<width; x++) {
                if (posSet.Contains(new Vector2Int(x,y))) {
                    colorMap[y * width + x] = Color.white;
                    continue;
                }
                colorMap[y * width + x] = Color.black;
            }
        }
        return colorMap;
    } 

    void Vector2IntSetAddToColorMap(int width, int height, HashSet<Vector2Int> posSet, ref Color[] colorMap) {
        for (int y=0; y<height; y++) {
            for (int x=0; x<width; x++) {
                if (posSet.Contains(new Vector2Int(x,y))) {
                    colorMap[y * width + x] = Color.white;
                }
            }
        }
    }
}
