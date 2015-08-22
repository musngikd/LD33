using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DialogueEditor : EditorWindow {
	public static DialogueEditor currentEditor;
	public Dialogue dialogue;
	public DialogueReader reader;
	private int instanceId;
	public Vector2 mousePos;

	private Output selectedOutput;
	private Node selectedNode;
	private Element selectedElement;
	private Node tempNode;
	private Element tempElement;

	private int propertySize = 270;
	
	private int cellSize;
	private Vector2 nodeSize = new Vector2(5,1);
	private Vector2 outputSize = new Vector2(1,1);
	private Vector2 gridSize = new Vector2(200,200);

	Vector2 propertiesScroll = new Vector2(0,0);
	
	public static void InitEditorWindow(Object obj){
		currentEditor = (DialogueEditor)EditorWindow.GetWindow<DialogueEditor>();
		currentEditor.instanceId = obj.GetInstanceID();
		currentEditor.title = "Node Editor";
		currentEditor.reader = (DialogueReader)GameObject.FindObjectOfType(typeof(DialogueReader));
	}
	
	void OnGUI(){
		dialogue = (Dialogue)EditorUtility.InstanceIDToObject(instanceId);
		if (dialogue != null){
			cellSize = (int)(20 * dialogue.zoom);
			mousePos = (Event.current.mousePosition + dialogue.gridPosition) / cellSize;
			DrawMain();
			DrawProperties();
			ProcessEvents(Event.current);
			Repaint();
		}
	}

	//Draws the main grid area
	void DrawMain (){
		Rect gridRect = new Rect(-dialogue.gridPosition.x, -dialogue.gridPosition.y, gridSize.x * cellSize, gridSize.y * cellSize);
		GUILayout.BeginArea (gridRect);
		DrawGrid (gridRect, cellSize * 5, 0.15f, Color.white);
		DrawGrid (gridRect, cellSize, 0.05f, Color.white);
		SetRects();
		DrawButtons();
		DrawLines();
		DrawNodes();
		GUILayout.EndArea ();
		GUI.Box (new Rect(10,position.height - 30, 100, 20),"X:" + (int)mousePos.x + " Y:" + (int)mousePos.y);
		dialogue.zoom = GUI.HorizontalSlider (new Rect((position.width - propertySize)/2 - 100,position.height - 30, 200, 20), dialogue.zoom, 0.5f, 2);
	}
	
	//Sets position and size for all nodes, elements, and outputs
	void SetRects(){
		foreach (Node node in dialogue.nodes) {
			float relativeHeight = nodeSize.y;
			Vector2 snapPos = new Vector2(Mathf.Round(node.position.x) * cellSize, Mathf.Round(node.position.y) * cellSize);
			float nodeWidth = nodeSize.x * cellSize;
			float nodeHeight = nodeSize.y * cellSize;
			float outputWidth = outputSize.x * cellSize;
			float outputHeight = outputSize.y * cellSize;
			
			node.input.rect = new Rect (snapPos.x - outputWidth, snapPos.y, outputWidth, outputHeight);
			
			foreach(Redirection redirection in node.redirections){
				if (redirection.automatic){
					redirection.rect = new Rect(snapPos.x, snapPos.y + relativeHeight * cellSize, nodeWidth, nodeHeight);
					redirection.output.rect = new Rect(snapPos.x + nodeWidth, snapPos.y + relativeHeight * cellSize, outputWidth, outputHeight);
					relativeHeight += nodeSize.y;
				}
			}
			
			foreach(Action action in node.actions){
				action.rect = new Rect(snapPos.x, snapPos.y + relativeHeight * cellSize, nodeWidth, nodeHeight);
				relativeHeight += nodeSize.y;
			}
			
			foreach(Line line in node.lines){
				line.rect = new Rect(snapPos.x, snapPos.y + relativeHeight * cellSize, nodeWidth, nodeHeight);
				relativeHeight += nodeSize.y;
			}
			foreach(Option option in node.options){
				option.rect = new Rect(snapPos.x, snapPos.y + relativeHeight * cellSize, nodeWidth, nodeHeight);
				option.output.rect = new Rect(snapPos.x + nodeWidth, snapPos.y + relativeHeight * cellSize, outputWidth, outputHeight);
				relativeHeight += nodeSize.y;
			}
			foreach(Redirection redirection in node.redirections){
				if (!redirection.automatic){
					redirection.rect = new Rect(snapPos.x, snapPos.y + relativeHeight * cellSize, nodeWidth, nodeHeight);
					redirection.output.rect = new Rect(snapPos.x + nodeWidth, snapPos.y + relativeHeight * cellSize, outputWidth, outputHeight);
					relativeHeight += nodeSize.y;
				}
			}
			
			node.output.rect = new Rect (snapPos.x + nodeWidth, snapPos.y + relativeHeight * cellSize, outputWidth, outputHeight);
			relativeHeight += nodeSize.y;
			node.rect = new Rect(snapPos.x, snapPos.y, nodeWidth, relativeHeight * cellSize);
		}
	}

	//Draws all nodes and elements in dialogue
	void DrawNodes(){
		int fontSize = (int)(11 * dialogue.zoom);
		int max = 12;
		foreach (Node node in dialogue.nodes) {
			Rect top = new Rect(node.rect.x, node.rect.y, nodeSize.x * cellSize, nodeSize.y * cellSize);
			Rect bottom = new Rect(node.rect.x, node.rect.y + node.rect.height - nodeSize.y * cellSize, nodeSize.x * cellSize, nodeSize.y * cellSize);
			if (node.nodeID == 0){
				GUI.Label(top, "Start", NewStyle("toporange", fontSize, Color.black, FontStyle.BoldAndItalic));
				GUI.Label(bottom, "", NewStyle("bottomorange", fontSize, Color.black, FontStyle.Normal));
			}
			else{
				string actorName = "Null";
				if (reader != null){
					Actor currentActor = reader.actors.Find(x => x.actorID == node.actorID);
					actorName = (currentActor != null) ? currentActor.name : "No Actor";
				}
				
				GUI.Label(top, actorName, NewStyle("topblue", fontSize, Color.black, FontStyle.BoldAndItalic));
				foreach(Line line in node.lines){
					string text = (string.IsNullOrEmpty(line.text) ? "..." : line.text.Trim());
					text = (text.Length < max) ? text : (text.Substring(0, max - 2) + "...");
					GUI.Label(line.rect, text, NewStyle("middlelightblue", fontSize, Color.black, FontStyle.Bold));
				}
				foreach(Option option in node.options){
					string text = (string.IsNullOrEmpty(option.text) ? "..." : option.text.Trim());
					text = (text.Length < max) ? text : (text.Substring(0, max - 2) + "...");
					GUI.Label(option.rect, text, NewStyle("middlered", fontSize, Color.black, FontStyle.Bold));
				}	
				foreach(Redirection redirection in node.redirections)
					GUI.Label(redirection.rect, "Redirect", NewStyle("middlegreen", fontSize, Color.black, FontStyle.Bold));
				foreach(Action action in node.actions){
					string text = "";
					if (action.operation == Action.Operation.Add)
						text = action.name + " + " + action.number;
					else if (action.operation == Action.Operation.Subtract)
						text = action.name + " - " + action.number;
					else if (action.operation == Action.Operation.Set)
						text = action.name + " = " + action.number;
					text = (text.Length < max) ? text : (text.Substring(0, max - 2) + "...");
					GUI.Label(action.rect, text, NewStyle("middleyellow", fontSize, Color.black, FontStyle.Bold));
				}
				GUI.Label(bottom, "", NewStyle("bottomblue", fontSize, Color.black, FontStyle.Normal));
			}
		}
	}
	
	//Draws all input/output buttons for all nodes
	void DrawButtons(){
		foreach(Node node in dialogue.nodes){
			if (node.nodeID == 0)
				DrawOutputButton(node.output, "outputorange");
			else{
				DrawInputButton(node);
				DrawOutputButton(node.output, "outputblue");
			}
			foreach (Option option in node.options)
				DrawOutputButton(option.output, "outputred");
			foreach (Redirection redirection in node.redirections)
				DrawOutputButton(redirection.output, "outputgreen");
		}
	}
	
	//Draws a single output button of a node
	void DrawOutputButton(Output output, string image){
		if (GUI.Button (output.rect, "", NewStyle(image, 11, Color.black, FontStyle.Normal))){
			output.nextNodeID = -1;
			selectedOutput = output;
		}
	}
	
	//Draws a single input button of a node
	void DrawInputButton(Node node){
		if (GUI.Button (node.input.rect, "", NewStyle("inputblue", 11, Color.black, FontStyle.Normal))) {
			if (selectedOutput != null)
				selectedOutput.nextNodeID = node.nodeID;
			selectedOutput = null;
		}
	}
	
	//Draws lines for all ouputs in all nodes
	void DrawLines(){
		foreach (Node node in dialogue.nodes) {
			DrawOutputLine (node.output);
			foreach (Redirection redirection in node.redirections)
				DrawOutputLine (redirection.output);
			foreach (Option option in node.options)
				DrawOutputLine (option.output);
		}
		if (selectedOutput != null)
			DrawLine(selectedOutput.rect, new Rect(mousePos.x * cellSize, mousePos.y * cellSize, 1, 1), selectedOutput.color);
	}
	
	//Draws line from an output to its input if set
	void DrawOutputLine (Output output){
		if (output.nextNodeID != -1) {
			Node nextNode = dialogue.nodes.Find (x => x.nodeID == output.nextNodeID);
			if (nextNode != null)
				DrawLine(output.rect, nextNode.input.rect, output.color);
		}
	}
	
	//Draws properties panel - used to view properties of selected element
	void DrawProperties(){
		Rect propertyRect = new Rect(position.width - propertySize, 0, propertySize, position.height);
		GUI.Box (propertyRect, "", NewStyle("backgrounddark", 0,Color.black, FontStyle.Normal));
		int spacer = 5;
		Rect propertyInnerRect = new Rect(position.width - propertySize + spacer, spacer, propertySize - spacer * 2, position.height - spacer * 2);
		GUI.Box (propertyInnerRect, "Properties", NewStyle("background", 20, Color.white, FontStyle.BoldAndItalic));
		GUILayout.BeginArea(propertyInnerRect);
		GUILayout.Space(60);
		propertiesScroll = GUILayout.BeginScrollView(propertiesScroll);
		GUILayout.BeginHorizontal();
		GUILayout.Space(20);
		GUILayout.BeginVertical();
		if (selectedElement != null){
			switch(selectedElement.GetType().ToString()){
				case "Line":
					GUILayout.Label("Line", NewStyle("", 14, Color.white, FontStyle.Bold));
					GUILayout.Space(20);
					(selectedElement as Line).text = GUILayout.TextArea((selectedElement as Line).text, GUILayout.MinHeight(60));	
					break;
				case "Option":
					GUILayout.Label("Option", NewStyle("", 14, Color.white, FontStyle.Bold));
					GUILayout.Space(20);
					(selectedElement as Option).text = GUILayout.TextArea((selectedElement as Option).text, GUILayout.MinHeight(40));
					break;
				case "Redirection":
					GUILayout.Label("Redirection", NewStyle("", 14, Color.white, FontStyle.Bold));
					GUILayout.Space(20);
					(selectedElement as Redirection).automatic = EditorGUILayout.Toggle("Automatic: ", (selectedElement as Redirection).automatic);
					break;
				case "Action":
					GUILayout.Label("Action", NewStyle("", 14, Color.white, FontStyle.Bold));
					GUILayout.Space(20);
					Action action = selectedElement as Action;
					action.name = EditorGUILayout.TextField("Key: ", action.name);
					action.operation = (Action.Operation)EditorGUILayout.EnumPopup("Action: ", action.operation);
					action.number = EditorGUILayout.IntField("Number: ", action.number);
					break;
			}
			DrawConditionsList(selectedElement.conditions);
		}
		else if (selectedNode != null && selectedNode.nodeID != 0){
			GUILayout.Label("Node", NewStyle("", 14, Color.white, FontStyle.Bold));
			GUILayout.Space(20);
			if (reader != null){
				List<string> names = new List<string>();
				names.Add("No Actor");
				foreach (Actor actor in reader.actors)
					names.Add(actor.name);
				int index = reader.actors.FindIndex(x => x.actorID == selectedNode.actorID) + 1;
				index = EditorGUILayout.Popup(index, names.ToArray());
				selectedNode.actorID = (index > 0) ? reader.actors[index - 1].actorID : 0;
			}
			else{
				EditorGUILayout.LabelField("No Dialogue Reader Found.");
				if (GUILayout.Button("Refresh"))
					reader = (DialogueReader)GameObject.FindObjectOfType(typeof(DialogueReader));
			}
		}
		GUILayout.EndVertical();
		GUILayout.Space(20);
		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}
	
	//Draws condition list/controls
	void DrawConditionsList(List<Condition> list){
		GUILayout.Space(20);
		GUILayout.BeginHorizontal();
		GUILayout.Label("Conditions");
		GUILayout.Label("" + list.Count);
		if(GUILayout.Button("-") && list.Count  > 0)
			list.RemoveAt(list.Count - 1);
		if(GUILayout.Button("+"))
			list.Add(new Condition());
		GUILayout.EndHorizontal();
		for (int i = 0; i < list.Count; i++){
			list[i].show = EditorGUILayout.Foldout(list[i].show, "Condition " + (i + 1));
			if (list[i].show){
				EditorGUI.indentLevel ++;
				EditorGUIUtility.labelWidth = 100;
				list[i].name = EditorGUILayout.TextField("Key: ", list[i].name);
				list[i].operation = (Condition.Operation)EditorGUILayout.EnumPopup("Test: ", list[i].operation);
				list[i].number = EditorGUILayout.IntField("Num: ", list[i].number);
				EditorGUI.indentLevel --;
			}
		}
	}

	//Processes user input including selection, movement and menus
	void ProcessEvents(Event e){
		bool showStart = true;
		Rect window = new Rect(0, 0, position.width - propertySize, position.height);
		if (window.Contains(e.mousePosition)){
			if ((e.button == 0 || e.button == 1) && e.type == EventType.MouseDown){
				tempNode = null;
				tempElement = null;
				selectedOutput = null;
				foreach (Node node in dialogue.nodes){
					if (node.nodeID == 0)
						showStart = false;
					if (node.rect.Contains(mousePos * cellSize))
						tempNode = node;
					ContainsElement(node.lines, ref tempElement);
					ContainsElement(node.options, ref tempElement);
					ContainsElement(node.redirections, ref tempElement);
					ContainsElement(node.actions, ref tempElement);
				}
				if (e.button == 0){
					selectedElement = tempElement;
					selectedNode = tempNode;
				}
				if (e.button == 1)
					ProcessContextMenu(e, showStart);
			}
	
			if (e.button == 0 && e.type == EventType.MouseDrag){
				if(selectedNode != null){
					selectedNode.position += e.delta/cellSize;
					selectedNode.position.x = Mathf.Clamp(selectedNode.position.x, 0, gridSize.x - nodeSize.x);
					selectedNode.position.y = Mathf.Clamp(selectedNode.position.y, 0, gridSize.y - nodeSize.y);
				}
				else{
					dialogue.gridPosition -= e.delta;
					dialogue.gridPosition.x = Mathf.Clamp(dialogue.gridPosition.x, 0, gridSize.x * cellSize - position.width + propertySize);
					dialogue.gridPosition.y = Mathf.Clamp(dialogue.gridPosition.y, 0, gridSize.y * cellSize - position.height);
				}
			}
		}
	}

	//Context menu used for dialogue, node, and element control/commands
	void ProcessContextMenu(Event e, bool showStart){
		GenericMenu menu = new GenericMenu();
		if (tempNode != null){
			if (tempNode.nodeID != 0){
				if (tempElement != null){
					menu.AddItem(new GUIContent("Delete " + tempElement.GetType().ToString()), false, ContextCallBack, "7");
					menu.AddItem(new GUIContent("Duplicate " + tempElement.GetType().ToString()), false, ContextCallBack, "8");
					menu.AddSeparator("");
				}
				menu.AddItem(new GUIContent("Add Line"), false, ContextCallBack, "0");
				menu.AddItem(new GUIContent("Add Option"), false, ContextCallBack, "1");
				menu.AddItem(new GUIContent("Add Redirection"), false, ContextCallBack, "2");
				menu.AddItem(new GUIContent("Add Action"), false, ContextCallBack, "3");
				menu.AddSeparator("");
			}
			menu.AddItem(new GUIContent("Move to Back"), false, ContextCallBack, "9");
			menu.AddItem(new GUIContent("Move to Front"), false, ContextCallBack, "10");
			menu.AddItem(new GUIContent("Delete Node"), false, ContextCallBack, "6");
		}
		else{
			menu.AddItem(new GUIContent("Create Node"), false, ContextCallBack, "5");
			if (showStart)
				menu.AddItem(new GUIContent("Create Start Node"), false, ContextCallBack, "4");
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Save Dialogue"), false, ContextCallBack, "11");
			menu.AddItem(new GUIContent("Load Dialogue"), false, ContextCallBack, "12");
		}
		menu.ShowAsContext();
		e.Use();
	}
	
	//Used with context menu to call functions
	void ContextCallBack(object obj){
		if (obj.ToString() == "0")
			tempNode.lines.Add(new Line());
		else if (obj.ToString() == "1")
			tempNode.options.Add(new Option());
		else if (obj.ToString() == "2")
			tempNode.redirections.Add(new Redirection());
		else if (obj.ToString() == "3")
			tempNode.actions.Add(new Action());
		else if (obj.ToString() == "4")
			CreateStart();
		else if (obj.ToString() == "5")
			CreateNode();
		else if (obj.ToString() == "6")
			DeleteNode(tempNode);
		else if (obj.ToString() == "7")
			DeleteElement(tempElement, tempNode);
		else if (obj.ToString() == "8")
			DuplicateElement(tempElement, tempNode);
		else if (obj.ToString() == "9"){
			dialogue.nodes.Remove(tempNode);
			dialogue.nodes.Insert(0, tempNode);
		}
		else if (obj.ToString() == "10"){
			dialogue.nodes.Remove(tempNode);
			dialogue.nodes.Insert(dialogue.nodes.Count, tempNode);
		}
		else if (obj.ToString() == "11")
			SaveDialogue(dialogue);
		else if (obj.ToString() == "12")
			LoadDialogue(dialogue);
	}
	
	//Deletes given element from given node
	void DeleteElement(Element element, Node node){
		string type = element.GetType().ToString();
		if (type == "Line")
			node.lines.Remove((Line)element);
		else if (type == "Option")
			node.options.Remove((Option)element);
		else if (type == "Redirection")
			node.redirections.Remove((Redirection)element);
		else if (type == "Action")
			node.actions.Remove((Action)element);
	}
	
	//Duplicates element and adds it to appropriate element list
	void DuplicateElement(Element element, Node node){
		string type = element.GetType().ToString();
		if (type == "Line")
			node.lines.Add(new Line((Line)element));
		else if (type == "Option")
			node.options.Add(new Option((Option)element));
		else if (type == "Redirection")
			node.redirections.Add(new Redirection((Redirection)element));
		else if (type == "Action")
			node.actions.Add(new Action((Action)element));
	}
	
	//Sets ref element if mouse position is inside element rect
	void ContainsElement<T>(List<T> list, ref Element refElement) where T : Element{
		foreach(Element element in list){
			if (element.rect.Contains(mousePos * cellSize))
				refElement = element;
		}
	}
	
	//Creates new node with autonum id
	void CreateNode(){
		Node currentNode = new Node();
		currentNode.lines.Add(new Line());
		currentNode.position = mousePos;
		bool valid = false;
		while (valid == false){
			valid = true;
			foreach (Node node in dialogue.nodes){
				if (node.nodeID == dialogue.autoNumNode) 
					valid = false;
			}
			if (valid == false)
				dialogue.autoNumNode ++;
		}
		currentNode.nodeID = dialogue.autoNumNode;
		dialogue.autoNumNode ++;
		currentNode.actorID = 0;
		dialogue.nodes.Add(currentNode);
	}
	
	//Creates start node
	void CreateStart(){
		Node currentNode = new Node();
		currentNode.position = mousePos;
		currentNode.nodeID = 0;
		currentNode.output.color = new Color(1, 0.5f, 0);
		dialogue.nodes.Add(currentNode);
	}
	
	//Deletes given node
	void DeleteNode(Node deleteNode){
		dialogue.nodes.Remove(deleteNode);
		foreach (Node node in dialogue.nodes){
			if (node.output.nextNodeID == deleteNode.nodeID)
				node.output.nextNodeID = -1;
			foreach (Option option in node.options){
				if (option.output.nextNodeID == deleteNode.nodeID)
					option.output.nextNodeID = -1;
			}
			foreach (Redirection redirection in node.redirections){
				if (redirection.output.nextNodeID == deleteNode.nodeID)
					redirection.output.nextNodeID = -1;
			}
		}
	}
	
	//Gets style used for node and inspector text
	public static GUIStyle NewStyle(string texture, int fontSize, Color color, FontStyle fontStyle){
		GUIStyle style = new GUIStyle();
		style.normal.textColor = color;
		style.normal.background = (Texture2D)Resources.Load("Textures/" + texture, typeof(Texture2D));
		style.wordWrap = true;
		style.fontSize = fontSize;
		style.fontStyle = fontStyle;
		style.alignment = (color == Color.white) ? TextAnchor.UpperCenter : TextAnchor.MiddleCenter;
		return style;	
	}
	
	//Saves dialogue to file
	static void SaveDialogue(Dialogue dialogue){
		string savePath = EditorUtility.SaveFilePanel("Load Dialogue",Application.dataPath,"", "asset");
		if(savePath.Length != 0) {
			ScriptableObject saveFile = ScriptableObject.CreateInstance<SaveFile>();
			EditorUtility.CopySerialized(dialogue, saveFile);
			AssetDatabase.CreateAsset(saveFile, savePath.Substring(Application.dataPath.Length - 6));
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
	
	//Copies dialogue from file to given dialogue
	static void LoadDialogue(Dialogue dialogue){
		string loadPath = EditorUtility.OpenFilePanel("Load Dialogue", Application.dataPath, "asset");
		if (loadPath.Length != 0){
			SaveFile saveFile = (SaveFile)AssetDatabase.LoadAssetAtPath(loadPath.Substring(Application.dataPath.Length - 6), typeof(SaveFile));
			if (saveFile != null)
				EditorUtility.CopySerialized(saveFile, dialogue);
			else
				Debug.Log("Bag :  " + loadPath.Substring(Application.dataPath.Length - 6));
		}
	}
	
	//Draws a bezier line between two points
	static void DrawLine(Rect fromRect, Rect toRect, Color color){
		Vector3 from = new Vector3 (fromRect.x + fromRect.width/3, fromRect.y + fromRect.height/2, 0);
		Vector3 to = new Vector3 (toRect.x + toRect.width - toRect.width/3, toRect.y + toRect.height/2, 0);
		Vector3 startTan = from + Vector3.right * 30;
		Vector3 endTan = to + Vector3.left * 30;
		Handles.DrawBezier(from, to, startTan, endTan, color, null, 4);
	}
	
	//draws a grid given a rect and spacing
	public static void DrawGrid(Rect viewRect, float grideSpacing, float gridOpacity, Color gridColor){
		int widthDivs = Mathf.CeilToInt(viewRect.width/grideSpacing);
		int heightDivs = Mathf.CeilToInt(viewRect.height/grideSpacing);
		Handles.BeginGUI();
		Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
		for(int x = 0; x < widthDivs; x++)
			Handles.DrawLine(new Vector3(grideSpacing * x, 0, 0), new Vector3(grideSpacing * x, viewRect.height, 0));
		for(int y = 0; y < heightDivs; y++)
			Handles.DrawLine(new Vector3(0, grideSpacing * y, 0), new Vector3(viewRect.width, grideSpacing * y, 0));
		Handles.color = Color.white;
		Handles.EndGUI();
	}
}