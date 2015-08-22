using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ActorEditor : EditorWindow {
	public static ActorEditor currentEditor;
	public DialogueReader reader;
	private int instanceId;
	private Actor selectedActor;
	Vector2 scrollPos = new Vector2(0,0);
	
	public static void InitEditorWindow(Object obj){
		currentEditor = (ActorEditor)EditorWindow.GetWindow<ActorEditor>();
		currentEditor.instanceId = obj.GetInstanceID();
		currentEditor.title = "Actor Editor";
		currentEditor.reader = (DialogueReader)EditorUtility.InstanceIDToObject(currentEditor.instanceId);
		if (currentEditor.reader != null && currentEditor.reader.actors.Count > 0)
			currentEditor.selectedActor = currentEditor.reader.actors[0];
	}
	
	void OnGUI(){
		reader = (DialogueReader)EditorUtility.InstanceIDToObject(instanceId);
		if (reader != null)
			DrawActors();
		if (GUI.changed)
			EditorUtility.SetDirty(reader);
	}
	
	//moves actor up or down in the actor list
	void MoveActor(Actor actor, string direction){
		int index = reader.actors.IndexOf(actor);
		if (direction == "up" && index > 0){
			reader.actors.Remove(actor);
			reader.actors.Insert(index - 1, actor);
		}	
		if (direction == "down" && index < reader.actors.Count - 1){
			reader.actors.Remove(actor);
			reader.actors.Insert(index + 1, actor);
		}	
	}
	
	//Draws actor list, and properties for selected actor
	void DrawActors(){
		float spacer = 10;
		GUILayout.BeginArea(new Rect(spacer, spacer, position.width - spacer * 2, position.height - spacer * 2));
			GUILayout.BeginHorizontal();
				GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(position.width * 0.4f - spacer * 2));
					EditorGUILayout.LabelField("Actor List", DialogueEditor.NewStyle("", 15, Color.white, FontStyle.Bold));
					GUILayout.Space(spacer);
					scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
						for (int i = 0; i < reader.actors.Count; i++){
							Actor actor = reader.actors[i];
							GUIStyle style = (selectedActor == actor) ? EditorStyles.boldLabel : EditorStyles.label;
							GUILayout.BeginHorizontal();
								if(GUILayout.Button(actor.name, style)){
									selectedActor = actor;
									GUI.FocusControl("");
								}
								if(GUILayout.Button("▲", GUILayout.Width(25)))
									MoveActor(actor, "up");
								if(GUILayout.Button("▼", GUILayout.Width(25)))
									MoveActor(actor, "down");
								if(GUILayout.Button("Delete", GUILayout.Width(50)))
									DeleteActor(actor);
							GUILayout.EndHorizontal();
						}
					EditorGUILayout.EndScrollView();
					GUILayout.FlexibleSpace();
					if(GUILayout.Button("Create Actor"))
						CreateActor();
				GUILayout.EndVertical();
				GUILayout.Space(spacer);
				GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(position.width * 0.6f - spacer));
				EditorGUILayout.LabelField("Actor Properties", DialogueEditor.NewStyle("", 15, Color.white, FontStyle.Bold));
					GUILayout.Space(spacer);
					if (selectedActor != null){
						selectedActor.name = EditorGUILayout.TextField("Name: ", selectedActor.name);
						selectedActor.positionType = (Actor.PositionType)EditorGUILayout.EnumPopup("Position Type ", selectedActor.positionType);
						if (selectedActor.positionType == Actor.PositionType.Dynamic){
							selectedActor.transform = (Transform)EditorGUILayout.ObjectField("Transform: ", selectedActor.transform, typeof(Transform), true);
							selectedActor.autoPosition = EditorGUILayout.Toggle("Auto Position: ", selectedActor.autoPosition);
							selectedActor.offset = EditorGUILayout.Vector3Field("Offset", selectedActor.offset);
						}
						else{
							selectedActor.position = (TextAnchor)EditorGUILayout.EnumPopup("Position ", selectedActor.position);
							selectedActor.offset = EditorGUILayout.Vector2Field("Offset", selectedActor.offset);
						}
						selectedActor.image = (Texture2D)EditorGUILayout.ObjectField("Image: ", selectedActor.image, typeof(Texture2D), true);
						if (selectedActor.image)
							selectedActor.imageRight = EditorGUILayout.Toggle("Switch Side: ", selectedActor.imageRight);
			
						GUILayout.FlexibleSpace();
					}
					else
						GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	
	//Creates a new actor with autonum id
	void CreateActor(){
		Actor newActor = new Actor();
		bool valid = false;
		while (valid == false){
			valid = true;
			foreach (Actor actor in reader.actors){
				if (actor.actorID == reader.autoNumActor) 
					valid = false;
			}
			if (valid == false)
				reader.autoNumActor ++;
		}
		newActor.actorID = reader.autoNumActor;
		newActor.name = "New Actor";
		reader.autoNumActor ++;
		reader.actors.Add(newActor);
		selectedActor = newActor;
	}
	
	//Deletes actor and deletes all references to that actor 
	void DeleteActor(Actor actor){
		reader.actors.Remove(actor);
		selectedActor = (reader.actors.Count > 0) ? reader.actors[0] : null;
		
		Dialogue[] dialogues = (Dialogue[])GameObject.FindObjectsOfType(typeof(Dialogue));
		foreach (Dialogue dialogue in dialogues){
			foreach (Node node in dialogue.nodes){
				if (node.actorID == actor.actorID)
					node.actorID = 0;
			}
		}
	}
}