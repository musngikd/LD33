using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class KeyEditor : EditorWindow {
	public static KeyEditor currentEditor;
	public DialogueReader reader;
	private int instanceId;
	
	Vector2 scrollPos = new Vector2(0,0);
	string newName = "";
	int newNum = 0;

	bool nameDesc = false;
	bool numDesc = false;
	
	public static void InitEditorWindow(Object obj){
		currentEditor = (KeyEditor)EditorWindow.GetWindow<KeyEditor>();
		currentEditor.instanceId = obj.GetInstanceID();
		currentEditor.title = "Key Editor";
	}
	
	//Draws editable list of keys
	void OnGUI(){
		reader = (DialogueReader)EditorUtility.InstanceIDToObject(instanceId);
		if (reader != null){
			int spacer = 10;			
			EditorGUIUtility.labelWidth = 80;
			GUILayout.BeginArea (new Rect(spacer, spacer, position.width - spacer * 2, position.height - spacer * 2), GUI.skin.box);
				EditorGUILayout.LabelField("Key List", DialogueEditor.NewStyle("", 15, Color.white, FontStyle.Bold));
				GUILayout.Space(spacer);
				EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Name", EditorStyles.label))
						SortKeys("name");
					if (GUILayout.Button("Number", EditorStyles.label, GUILayout.Width(position.width/5)))
						SortKeys("number");
				EditorGUILayout.EndHorizontal();
				scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
					for(int i = 0; i < reader.Keys.Count; i++){
						Key key = reader.Keys[i];
						EditorGUILayout.BeginHorizontal();
						key.name = EditorGUILayout.TextField(key.name);
						key.number = EditorGUILayout.IntField(key.number, GUILayout.Width(position.width/5));
						if (GUILayout.Button("Delete"))
							reader.Keys.Remove(key);
						EditorGUILayout.EndHorizontal();
					}
				EditorGUILayout.EndScrollView();
				GUILayout.FlexibleSpace();
				EditorGUILayout.BeginHorizontal();
					newName = EditorGUILayout.TextField(newName);
					newNum = EditorGUILayout.IntField(newNum, GUILayout.Width(position.width/5));
					if (GUILayout.Button("Create"))
						CreateNewKey(newName, newNum);
				EditorGUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
		if (GUI.changed)
			EditorUtility.SetDirty(reader);
	}
	
	//Sorts keys by name or number
	void SortKeys(string sortBy){
		if (sortBy == "name"){
			reader.Keys = (nameDesc) ? reader.Keys.OrderBy(o => o.name).ToList() : reader.Keys.OrderByDescending(o => o.name).ToList();
			nameDesc = !nameDesc;
		}
		else if (sortBy == "number"){
			reader.Keys = (numDesc) ? reader.Keys.OrderBy(o => o.number).ToList() : reader.Keys.OrderByDescending(o => o.number).ToList();
			numDesc = !numDesc;
		}
		GUI.FocusControl("");	
	}
	
	//creates new key given name and number
	void CreateNewKey(string name, int number){
		if (reader.Keys.Find(x => x.name == name) != null)
			reader.Keys.Find(x => x.name == name).number = number;
		else{
			reader.Keys.Add(new Key(name, number));
			newName = "";
			newNum = 0;
		}
		GUI.FocusControl("");
	}
}