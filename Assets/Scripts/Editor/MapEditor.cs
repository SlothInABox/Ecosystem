using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapGenerator map = target as MapGenerator;

        if (GUILayout.Button("Generate Terrain"))
        {
            map.GenerateMap();
        }

        if(GUILayout.Button("Generate Environment"))
        {
            map.GenerateEnvironment();
        }
    }
}
