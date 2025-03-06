using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaveSOBase))]
public class MyScriptEditor : Editor
{
    private bool showGroup1 = true;
    private bool showGroup2 = true;

    SerializedProperty subWavesList;
    SerializedProperty subWaveStylesList;
    SerializedProperty subWaveMultipliersList;
    //SerializedProperty timesToTriggerSubWavesList;

    SerializedProperty constantWavesList;
    SerializedProperty constantWaveStylesList;
    SerializedProperty constantWaveMultipliersList;
    //SerializedProperty timesToTriggerConstantWavesList;

    void OnEnable()
    {
        subWavesList = serializedObject.FindProperty("subWaves");
        subWaveStylesList = serializedObject.FindProperty("subWaveStyles");
        subWaveMultipliersList = serializedObject.FindProperty("subWaveMultipliers");
        //timesToTriggerSubWavesList = serializedObject.FindProperty("timesToTriggerSubWaves");

        constantWavesList = serializedObject.FindProperty("constantWaves");
        constantWaveStylesList = serializedObject.FindProperty("constantWaveStyles");
        constantWaveMultipliersList = serializedObject.FindProperty("constantWaveMultipliers");
        //timesToTriggerConstantWavesList = serializedObject.FindProperty("timesToTriggerConstantWaves");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("\nNOTE: WaveSOBase is controlled by an editor script\n", EditorStyles.boldLabel);

        WaveSOBase waveBase = (WaveSOBase)target;

        EditorGUILayout.LabelField("");

        showGroup1 = EditorGUILayout.Foldout(showGroup1, "Sub Waves");
        if (showGroup1)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(subWavesList, true);
            EditorGUILayout.PropertyField(subWaveStylesList, true);
            EditorGUILayout.PropertyField(subWaveMultipliersList, true);
            //EditorGUILayout.PropertyField(timesToTriggerSubWavesList, true);
            EditorGUI.indentLevel--;

            // For text or ints:
            //waveBase.timeToTriggerSubWaves = EditorGUILayout.TextField("Some String", waveBase.SomeString);
        }

        EditorGUILayout.LabelField("");

        showGroup2 = EditorGUILayout.Foldout(showGroup2, "Constant Waves");
        if (showGroup2)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(constantWavesList, true);
            EditorGUILayout.PropertyField(constantWaveStylesList, true);
            EditorGUILayout.PropertyField(constantWaveMultipliersList, true);
            //EditorGUILayout.PropertyField(timesToTriggerConstantWavesList, true);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.LabelField("");
        
        serializedObject.ApplyModifiedProperties();
    }
}