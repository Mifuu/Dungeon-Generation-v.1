using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//https://docs.unity3d.com/Manual/editor-PropertyDrawers.html
//https://nosuchstudio.medium.com/learn-unity-editor-scripting-property-drawers-part-2-6fe6097f1586

// RoomDrawer
[CustomPropertyDrawer(typeof(RoomComponent))]
public class RoomComponentPropertyDrawer : PropertyDrawer
{
    const float enumBoxWidth = 30f;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty componentProperty = property.FindPropertyRelative("cell");
        for(int i=0;i<componentProperty.arraySize;i++) {//iterate
        SerializedProperty childProperty = componentProperty.GetArrayElementAtIndex(i); //child of componentProperty
            int y = i/7;
            int x = i%7;
            float posX = position.min.x + enumBoxWidth * (x+1);
            float posY = position.min.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (y+1);
            Rect rectEnum = new Rect(posX, posY, enumBoxWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rectEnum, childProperty, GUIContent.none);
        }
        for(int i=-3;i<4;i++) {
            float posX = position.min.x + enumBoxWidth * (i+4);
            float posY = position.min.y;
            Rect rectEnum = new Rect(posX, posY, enumBoxWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(rectEnum, " " + i);
            posX = position.min.x;
            posY = position.min.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (-i+4);
            rectEnum = new Rect(posX, posY, enumBoxWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(rectEnum, " " + i);
        }
        
        /*
        SerializedProperty level1 = componentProperty.GetArrayElementAtIndex(0);
        rectEnum = new Rect(position.min.x, position.min.y, enumBoxWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(rectEnum, level1, GUIContent.none);

        rectEnum = new Rect(position.min.x+enumBoxWidth, position.min.y, enumBoxWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(rectEnum, "test");

        rectEnum = new Rect(position.min.x+enumBoxWidth*2, position.min.y, enumBoxWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(rectEnum, level1, GUIContent.none);
        */ 
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int totalLines = 7;
        return EditorGUIUtility.singleLineHeight * (totalLines+1) + EditorGUIUtility.standardVerticalSpacing * totalLines;
    }
}
