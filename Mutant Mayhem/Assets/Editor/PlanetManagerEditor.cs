using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(PlanetManager))]
public class PlanetManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector for other fields
        DrawDefaultInspector();

        // Get the target object
        PlanetManager manager = (PlanetManager)target;

        // Check if the dictionary exists
        if (manager.statMultipliers != null && manager.statMultipliers.Count > 0)
        {
            EditorGUILayout.LabelField("Stat Multipliers", EditorStyles.boldLabel);

            foreach (var kvp in manager.statMultipliers)
            {
                // Display the key (PlanetStatModifier) and value (float)
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(kvp.Key.ToString(), GUILayout.Width(200));
                EditorGUILayout.LabelField(kvp.Value.ToString(), GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.LabelField("Stat Multipliers Dictionary is empty.");
        }
    }
}