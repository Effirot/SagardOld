using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HealthBar))]
public class HealthBarBarEditor : Editor
{
    public override void OnInspectorGUI() {
        HealthBar edit = target as HealthBar;

        edit.State = 1;
    }
}
