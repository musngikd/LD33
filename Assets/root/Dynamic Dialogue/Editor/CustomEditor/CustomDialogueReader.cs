using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(DialogueReader))]
public class CustomDialogueReader : Editor {
	public override void OnInspectorGUI(){
		GUI.changed = false;
		//DrawDefaultInspector();
		DialogueReader reader = (DialogueReader)target;
		reader.show[0] = EditorGUILayout.Foldout(reader.show[0], "General Settings");
		if (reader.show[0]){
			EditorGUI.indentLevel ++;
			reader.mainTexture = (Texture2D)EditorGUILayout.ObjectField("Main Texture: ", reader.mainTexture, typeof(Texture2D), true);
			reader.widthStyle = (DialogueReader.SizeStyle)EditorGUILayout.EnumPopup("Width Style: ", reader.widthStyle);
			reader.heightStyle = (DialogueReader.SizeStyle)EditorGUILayout.EnumPopup("Height Style: ", reader.heightStyle);
			reader.nodeSize = EditorGUILayout.Vector2Field("Size: ", reader.nodeSize);
			reader.nodeBorder = EditorGUILayout.Vector2Field("Border: ", reader.nodeBorder);
			reader.defaultPosition = EditorGUILayout.Vector2Field("Default Position: ", reader.defaultPosition);
			reader.stickToEdges = EditorGUILayout.Toggle("Stick to Edges: ", reader.stickToEdges);
			EditorGUI.indentLevel --;
		}
		
		reader.show[1] = EditorGUILayout.Foldout(reader.show[1], "Content Settings");
		if (reader.show[1]){
			EditorGUI.indentLevel ++;
			reader.displayName = EditorGUILayout.Toggle("Display Name: ", reader.displayName);
			reader.displayImage = EditorGUILayout.Toggle("Display Image: ", reader.displayImage);
			reader.imageSize = EditorGUILayout.IntField("Image Size: ", reader.imageSize);
			reader.textIndent = EditorGUILayout.IntField("Text Indent: ", reader.textIndent);
			reader.optionIndent = EditorGUILayout.IntField("Option Indent: ", reader.optionIndent);
			reader.alignment = (TextAnchor)EditorGUILayout.EnumPopup("Alignment: ", reader.alignment);
			reader.font = (Font)EditorGUILayout.ObjectField("Font: ", reader.font, typeof(Font), true);
			reader.fontColor = EditorGUILayout.ColorField("Font Color: ", reader.fontColor);
			reader.fontSize = EditorGUILayout.IntField("Font Size: ", reader.fontSize);
			EditorGUI.indentLevel --;
		}
		
		reader.show[2] = EditorGUILayout.Foldout(reader.show[2], "Highlight Settings");
		if (reader.show[2]){
			EditorGUI.indentLevel ++;
			reader.highlightTexture = (Texture2D)EditorGUILayout.ObjectField("Highlight Texture: ", reader.highlightTexture, typeof(Texture2D), true);
			
			reader.stretchTexture = EditorGUILayout.Toggle("Stretch Texture: ", reader.stretchTexture);
			reader.highlightFontColor = EditorGUILayout.ColorField("Font Color: ", reader.highlightFontColor);
			EditorGUI.indentLevel --;
		}
		
		reader.show[3] = EditorGUILayout.Foldout(reader.show[3], "Extra Settings");
		if (reader.show[3]){
			EditorGUI.indentLevel ++;
			reader.extraTexture = (Texture2D)EditorGUILayout.ObjectField("Extra Texture: ", reader.extraTexture, typeof(Texture2D), true);
			reader.extraSize = EditorGUILayout.Vector2Field("Extra Size: ", reader.extraSize);
			reader.extraPosition = EditorGUILayout.Vector2Field("Extra Offset: ", reader.extraPosition);
			EditorGUI.indentLevel --;
		}
		
		reader.show[4] = EditorGUILayout.Foldout(reader.show[4], "Sound Settings");
		if (reader.show[4]){
			EditorGUI.indentLevel ++;
			reader.openNoise = (AudioClip)EditorGUILayout.ObjectField("Open Noise: ", reader.openNoise, typeof(AudioClip), true);
			reader.closeNoise = (AudioClip)EditorGUILayout.ObjectField("Close Noise: ", reader.closeNoise, typeof(AudioClip), true);
			reader.nextNoise = (AudioClip)EditorGUILayout.ObjectField("Next Noise: ", reader.nextNoise, typeof(AudioClip), true);
			reader.optionNoise = (AudioClip)EditorGUILayout.ObjectField("Option Noise: ", reader.optionNoise, typeof(AudioClip), true);
			EditorGUI.indentLevel --;
		}
		
		reader.show[5] = EditorGUILayout.Foldout(reader.show[5], "Demo Settings");
		if (reader.show[5]){
			EditorGUI.indentLevel ++;
			reader.demo = EditorGUILayout.Toggle("Demo: ", reader.demo);
			reader.demoTalkDistance = EditorGUILayout.FloatField("Demo Talk Distance: ", reader.demoTalkDistance);
			reader.demoTalkWidth = EditorGUILayout.FloatField("Demo Talk Width: ", reader.demoTalkWidth);
			reader.demoGUIText = (GUIText)EditorGUILayout.ObjectField("Demo GUIText: ", reader.demoGUIText, typeof(GUIText), true);
			reader.demoGUIColor = EditorGUILayout.ColorField("Demo GUIColor ", reader.demoGUIColor);
			reader.show[6] = EditorGUILayout.Foldout(reader.show[6], "Custom Keys");
			if (reader.show[6]){
				EditorGUI.indentLevel ++;
				reader.startKey = (KeyCode)EditorGUILayout.EnumPopup("Start Key: ", reader.startKey);
				reader.nextKey = (KeyCode)EditorGUILayout.EnumPopup("Next Key: ", reader.nextKey);
				reader.upKey = (KeyCode)EditorGUILayout.EnumPopup("Up Key: ", reader.upKey);
				reader.downKey = (KeyCode)EditorGUILayout.EnumPopup("Down Key: ", reader.downKey);
				EditorGUI.indentLevel --;
			}
			EditorGUI.indentLevel --;
		}	

		if(GUILayout.Button("Edit Actors"))
			ActorEditor.InitEditorWindow(reader);
		if(GUILayout.Button("Edit Keys"))
			KeyEditor.InitEditorWindow(reader);
			
		if (GUI.changed)
			EditorUtility.SetDirty(target);
	}
}