using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomComponent
{
    public enum Component {No, Yes, Right, Top, Left, Bottom}   //type of cell in the room, Direction indicate next room connection
    [ReadOnly]public static int radius = 3;
    public Component[] cell = new Component[7*7]; //radius = 3

    public Component[,] GetCell2d() {
        Component[,] output = new Component[7,7];
        for (int y=0;y<7;y++) {
            for (int x=0;x<7;x++) {
                output[x,y] = cell[y*7+x];
            }
        }
        return output;
    }




    //old
    public (int x, int y) FindAreaConnectToComponent(Component[,] cell ,string roomFilter) { //find and return coordinates of the room area that have the required connection
        bool right = roomFilter.Contains("R");
        bool top = roomFilter.Contains("T");
        bool left = roomFilter.Contains("L");
        bool bottom = roomFilter.Contains("B");
        //the perfect fit
        for (int y=1;y<6;y++) {
            for (int x=1;x<6;x++) {
                if (right && !(right && cell[x+1,y] == Component.Right)) continue;
                if (top && !(top && cell[x,y-1] == Component.Top)) continue;
                if (left && !(left && cell[x-1,y] == Component.Left)) continue;
                if (bottom && !(bottom && cell[x,y+1] == Component.Bottom)) continue;
                return (x,y);
            }
        }
        Debug.Log("Failed to find required area, roomFilter = " + roomFilter);
        return (int.MaxValue, int.MaxValue); //not found
    }

    public bool CheckAreaConnectToComponent(int x, int y, Component[,] cell, string roomFilter) { //find and return coordinates of the room area that have the required connection
        bool right = roomFilter.Contains("R");
        bool top = roomFilter.Contains("T");
        bool left = roomFilter.Contains("L");
        bool bottom = roomFilter.Contains("B");
        if (!(right && cell[x+1,y] == Component.Right)) return false;
        if (!(top && cell[x,y-1] == Component.Top)) return false;
        if (!(left && cell[x-1,y] == Component.Left)) return false;
        if (!(bottom && cell[x,y+1] == Component.Bottom)) return false;
        return true;
    }
}
