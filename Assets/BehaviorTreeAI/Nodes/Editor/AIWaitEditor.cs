using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace AI {
    [CustomNodeEditor(typeof(AIWait))]
    public class AIWaitEditor : NodeEditor {

        public override void OnBodyGUI() {
            serializedObject.Update();

            AISetVariable node = target as AISetVariable;

            GUILayout.BeginHorizontal();
            NodeEditorGUILayout.PortField(GUIContent.none, target.GetInputPort("input"), GUILayout.MinWidth(0));
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField(new GUIContent("Wait Range"));
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("waitTimeMin"), new GUIContent(""), GUILayout.MaxWidth(20f));
            float min = serializedObject.FindProperty("waitTimeMin").floatValue;
            float max = serializedObject.FindProperty("waitTimeMax").floatValue;
            EditorGUILayout.MinMaxSlider(ref min, ref max, 0f, 10f);
            serializedObject.FindProperty("waitTimeMin").floatValue = min;
            serializedObject.FindProperty("waitTimeMax").floatValue = max;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("waitTimeMax"), new GUIContent(""), GUILayout.MaxWidth(20f));
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}