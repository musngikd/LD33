using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(Dialogue)), CanEditMultipleObjects]
public class CustomDialogue : Editor {
	public override void OnInspectorGUI(){
		//DrawDefaultInspector();
		EditorGUILayout.LabelField("Nodes: ", "" + ((Dialogue)target).nodes.Count);
		if(GUILayout.Button("Edit Dialogue"))
			DialogueEditor.InitEditorWindow(target);
	}
}