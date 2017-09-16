using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Assets.Resources.Script.Monobehaviour.Generator;

[CustomEditor (typeof (MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = (MapGenerator)target;
        if(DrawDefaultInspector())
        {
            if(mapGenerator.autoUpdate)
            {
                mapGenerator.DrawMapInEditor();
            }
        }

        if(GUILayout.Button("Generate"))
        {
            mapGenerator.DrawMapInEditor();
        }
    }
}
