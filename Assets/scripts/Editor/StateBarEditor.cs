using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CustomEditor(typeof(HealthBar)), CanEditMultipleObjects]
public class StateBarEditor : Editor
{
    public SerializedProperty NowState;

    void OnEnable()
    {
        NowState = serializedObject.FindProperty("State");
    }

    public override void OnInspectorGUI () {
        serializedObject.Update();

        EditorGUILayout.PropertyField(NowState);

        serializedObject.ApplyModifiedProperties();
    }

}
