using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomManager))]
public class RoomManagerEditor : Editor
{
    public override void OnInspectorGUI() {
        RoomManager roomManager = (RoomManager)target;
        DrawDefaultInspector();//https://docs.unity3d.com/ScriptReference/Editor.DrawDefaultInspector.html
        if (GUILayout.Button("Generate")) {
            roomManager.GenerateLevel("Level 5");
            MapGenerator mapGen = FindObjectOfType<MapGenerator>();
            mapGen.GeneratePixelMap(4);
        }

        if (GUILayout.Button("Add")) {
            roomManager.GenerateRoom();
            MapGenerator mapGen = FindObjectOfType<MapGenerator>();
            mapGen.GeneratePixelMap(4);
        }

        if (GUILayout.Button("Debug Info")) {
            roomManager.RoomDebugInfo();
        }

        GUILayout.Label("Draw");
        if (GUILayout.Button("Draw 1 Pixel Map")) {
            MapGenerator mapGen = FindObjectOfType<MapGenerator>();
            mapGen.Generate1PixelMap();
        }

        if (GUILayout.Button("Draw Pixel Map")) {
            MapGenerator mapGen = FindObjectOfType<MapGenerator>();
            mapGen.GeneratePixelMap(4);
        }
    }
}
