using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace AI {
    [CustomNodeEditor(typeof(AISetVariable))]
    public class AISetVariableEditor : NodeEditor {

        public override void OnBodyGUI() {
            serializedObject.Update();

            AISetVariable node = target as AISetVariable;

            GUILayout.BeginHorizontal();
            NodeEditorGUILayout.PortField(GUIContent.none, target.GetInputPort("input"), GUILayout.MinWidth(0));
            NodeEditorGUILayout.PortField(GUIContent.none, target.GetOutputPort("output"), GUILayout.MinWidth(0));
            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("variableType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("variableName"), new GUIContent("Name:"));
            switch( node.variableType) {
                case AISetVariable.VariableType.Float:
                    serializedObject.FindProperty("fVariable").floatValue = EditorGUILayout.FloatField("Value:", serializedObject.FindProperty("fVariable").floatValue);
                    break;
                case AISetVariable.VariableType.String:
                    serializedObject.FindProperty("sVariable").stringValue = EditorGUILayout.TextField("Value:", serializedObject.FindProperty("sVariable").stringValue);
                    break;
                case AISetVariable.VariableType.Int:
                    serializedObject.FindProperty("iVariable").intValue = EditorGUILayout.IntField("Value:", serializedObject.FindProperty("iVariable").intValue);
                    break;
                case AISetVariable.VariableType.UnityObject:
                    serializedObject.FindProperty("uVariable").objectReferenceValue = EditorGUILayout.ObjectField("Value:",serializedObject.FindProperty("uVariable").objectReferenceValue, typeof(UnityEngine.Object),false);
                    break;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}