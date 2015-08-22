using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
///	Reads, Controls and Displays Dialogue.
/// Includes commands to Start, Stop and Continue dialogue.
/// </summary>
[System.Serializable]
public class DialogueReader : MonoBehaviour {
	private Dialogue dialogue;
	public List<Key> Keys = new List<Key>();				//a list of all dialogue keys held by the player
	public List<Actor> actors = new List<Actor>();
	public int autoNumActor = 1;
	private Node currentNode;
	private Actor currentActor;
	private int selectionIndex;

	public Texture2D mainTexture;
	public enum SizeStyle {DynamicPixel, DynamicPercent, StaticPixel, StaticPercent}
	public SizeStyle widthStyle;
	public SizeStyle heightStyle;
	public Vector2 nodeSize = new Vector2(50f, 50f);
	public Vector2 nodeBorder = new Vector2(10, 10);
	public Vector2 defaultPosition = new Vector2(0.5f, 0.5f);
	public bool stickToEdges = false;
	public Texture2D extraTexture;
	public Vector2 extraSize = new Vector2(15, 15);
	public Vector2 extraPosition;
	public Font font;
	public Color fontColor = Color.black;
	public TextAnchor alignment = TextAnchor.MiddleCenter;
	public int fontSize = 20;
	public Texture2D highlightTexture;
	public bool stretchTexture = false;
	public Color highlightFontColor = Color.black;
	public bool[] show = new bool[]{false, false, false, false, false, false, false};
	
	public AudioClip openNoise;
	public AudioClip closeNoise;
	public AudioClip nextNoise;
	public AudioClip optionNoise;
	
	public bool displayName = false;
	public int textIndent = 5;
	public int optionIndent = 10;
	
	public bool displayImage = false;
	public int imageSize = 100;
	
	//This is for demo purposes - Edit this code customise interactions with the Dialogue Reader
	//--------------------------------------------------------------------------------------------------------
	public bool demo = true;
	public float demoTalkDistance = 5;
	public float demoTalkWidth = 0.01f;
	public GUIText demoGUIText;
	public Color demoGUIColor = Color.red;
	public KeyCode startKey = KeyCode.E;
	public KeyCode nextKey = KeyCode.Mouse0;
	public KeyCode upKey = KeyCode.PageUp;
	public KeyCode downKey = KeyCode.PageDown;
	public Transform goblin;

	void Start() {
		GameObject gob = GameObject.Find ("Goblin");
		goblin = gob.transform;
	}

	void Update () {
		if (demo){
			RaycastHit hit;
//			Screen.lockCursor = true; 
			Color guiColor = Color.white;
			if (Physics.SphereCast(transform.position, demoTalkWidth, transform.forward, out hit, demoTalkDistance) && hit.transform.GetComponent<Dialogue>()){
				guiColor = demoGUIColor;
				if(Input.GetKeyDown(startKey))	
					OpenConversation(hit.transform.GetComponent<Dialogue>());
			}
			if (demoGUIText) demoGUIText.color = guiColor;
			if (Input.GetKeyDown(nextKey))
				NextNode();						//continue conversation with left mouse button
			if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetKeyDown(upKey))
				Selection("up");					//selection up with mouse wheel
			if (Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetKeyDown(downKey))
				Selection("down");				//selection down with mouse wheel
		}

		if(Input.GetKeyDown(startKey))	
			OpenConversation(goblin.GetComponent<Dialogue>());

		if (Input.GetKeyDown(nextKey))
			NextNode();		

		if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetKeyDown(upKey))
			Selection("up");					//selection up with mouse wheel

		if (Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetKeyDown(downKey))
			Selection("down");	
	}
	//--------------------------------------------------------------------------------------------------------
	
	//PUBLIC METHODS
	//opens conversation at a node with a given ID
	public void OpenConversation(Dialogue newDialogue, int nodeID){
		if (dialogue == null){
			if (openNoise && newDialogue.nodes.Find(x => x.nodeID == nodeID) != null) 
				AudioSource.PlayClipAtPoint(openNoise, transform.position);
			dialogue = newDialogue;
			Next(nodeID);
		}
	}

	//opens conversation at first line of dialogue in script file
	public void OpenConversation(Dialogue newDialogue){
		OpenConversation(newDialogue, 0);
	}
	
	//ends conversation
	public void CloseConversation(){
		if (closeNoise) AudioSource.PlayClipAtPoint(closeNoise, transform.position);
		dialogue = null;
	}
	
	//selection movement (up or down)
	public void Selection(string direction){
		if (dialogue != null){
			if (direction.Equals("down") && selectionIndex < Validate(currentNode.options).Count - 1){
				selectionIndex++;
				if (optionNoise) AudioSource.PlayClipAtPoint(optionNoise, transform.position);
			}	
			else if (direction.Equals("up") && selectionIndex > 0){
				selectionIndex--;
				if (optionNoise) AudioSource.PlayClipAtPoint(optionNoise, transform.position);
			}
		}
	}
	
	//moves to next node
	public void NextNode(){
		if (dialogue != null){
			int nextID = currentNode.output.nextNodeID;
			if (Validate(currentNode.options).Count > 0)
				nextID = Validate(currentNode.options)[selectionIndex].output.nextNodeID;
			else if (Validate(currentNode.redirections).Count > 0){
				foreach(Redirection redirection in Validate(currentNode.redirections)){
					if (!redirection.automatic){
						nextID = redirection.output.nextNodeID;
						break;
					}
				}
			}	
			if (nextNoise && dialogue.nodes.Find(x => x.nodeID == nextID) != null)
				AudioSource.PlayClipAtPoint(nextNoise, transform.position);
			Next(nextID);
		}
	}

	//displays dialogue node
	void OnGUI () {		
		if (dialogue != null)
			DrawDialogue();
	}
	
	void DrawImage(){
		GUILayout.BeginVertical(GUILayout.Width(imageSize));
		GUILayout.FlexibleSpace();
		GUILayout.Label(currentActor.image, GUILayout.Width(imageSize), GUILayout.Height(imageSize));
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
	}

	//Draws all lines and options on a given background
	void DrawDialogue(){
		GUIStyle textStyle = TextStyle();
		
		Vector2 contentSize = GetContentSize();
		Vector2 backgroundSize = new Vector2(contentSize.x + nodeBorder.x * 2, contentSize.y + nodeBorder.y * 2);
		Vector2 backgroundPos = GetPosition(backgroundSize);
		Vector2 contentPos = new Vector2(backgroundPos.x + nodeBorder.x, backgroundPos.y + nodeBorder.y);

		GUI.DrawTexture(new Rect(backgroundPos.x, backgroundPos.y, backgroundSize.x, backgroundSize.y), mainTexture);
		
		GUILayout.BeginArea(new Rect(contentPos.x, contentPos.y, contentSize.x, contentSize.y));
			GUILayout.BeginHorizontal();
				if (displayImage && currentActor != null && currentActor.image && !currentActor.imageRight)
					DrawImage();
				
				GUILayout.BeginVertical (GUILayout.Width(contentSize.x - ((displayImage && currentActor != null && currentActor.image) ? imageSize : 0)));
				
					if (displayName && currentActor != null)
						GUILayout.Label (currentActor.name, textStyle);
					
					if (alignment != TextAnchor.UpperCenter && alignment != TextAnchor.UpperLeft && alignment != TextAnchor.UpperRight)
						GUILayout.FlexibleSpace ();
					
					List<Line> lines = Validate (currentNode.lines);
					if (lines.Count > 0){
						GUILayout.BeginHorizontal();
							GUILayout.Space(textIndent);
							GUILayout.BeginVertical();
								foreach (Line line in lines)
									GUILayout.Label (line.text, textStyle);
							GUILayout.EndVertical();
						GUILayout.EndHorizontal();
					}
					
					List<Option> options = Validate (currentNode.options);
					if (options.Count > 0){
						GUILayout.BeginHorizontal();
							GUILayout.Space(optionIndent);
							GUILayout.BeginVertical();
								for (int i = 0; i < options.Count; i++) {
									GUI.SetNextControlName ("" + i);
									Rect optionRect = GUILayoutUtility.GetRect(new GUIContent(options [i].text), textStyle);
									if (Event.current.type == EventType.Repaint && highlightTexture && i == selectionIndex) {
										Vector2 highlightSize = new Vector2 (optionRect.width + optionIndent, optionRect.height);
										if (!stretchTexture) {
											highlightSize.y = textStyle.CalcSize (new GUIContent (" ")).y;
											highlightSize.x = highlightTexture.width / highlightTexture.height * highlightSize.y;
										}
										GUI.DrawTexture (new Rect (optionRect.x - optionIndent, optionRect.y, highlightSize.x, highlightSize.y), highlightTexture);
									}
									GUI.Button(optionRect, options [i].text, textStyle);
								}
								GUI.FocusControl ((options.Count > 0) ? "" + selectionIndex : "");
							GUILayout.EndVertical();
						GUILayout.EndHorizontal();	
					}
					
					if (alignment != TextAnchor.LowerCenter && alignment != TextAnchor.LowerLeft && alignment != TextAnchor.LowerRight)
						GUILayout.FlexibleSpace ();
				
				GUILayout.EndVertical ();
		
				if (displayImage && currentActor != null && currentActor.image && currentActor.imageRight)
					DrawImage();
			GUILayout.EndHorizontal();		
		GUILayout.EndArea();
		
		if (extraTexture){
			Vector2 position = new Vector2(backgroundPos.x + extraPosition.x * backgroundSize.x, backgroundPos.y + extraPosition.y * backgroundSize.y);
			GUI.DrawTexture(new Rect(position.x, position.y, extraSize.x, extraSize.y), extraTexture);
		}
	}

	//Gets total size of all lines and options
	Vector2 GetContentSize(){
		float extra = (displayImage && currentActor != null && currentActor.image) ? imageSize + nodeBorder.x : 0;
		Vector2 size = new Vector2(0,0);
		
		GUIStyle textStyle = TextStyle();
		if (widthStyle == SizeStyle.StaticPercent)
			size.x = (Screen.width * nodeSize.x/100) - nodeBorder.x * 2;
		else if (widthStyle == SizeStyle.StaticPixel)
			size.x = nodeSize.x - nodeBorder.x * 2;
		else if (widthStyle == SizeStyle.DynamicPercent || widthStyle == SizeStyle.DynamicPixel){
			float max = (widthStyle == SizeStyle.DynamicPercent) ? Screen.width * nodeSize.x/100 - nodeBorder.x * 2: nodeSize.x - nodeBorder.x * 2;
			if (displayName && currentActor != null)
				size.x = Mathf.Clamp(textStyle.CalcSize(new GUIContent(currentActor.name)).x + extra, size.x, max);
			foreach(Line line in Validate(currentNode.lines))
				size.x = Mathf.Clamp(textStyle.CalcSize(new GUIContent(line.text)).x + extra + textIndent, size.x, max);
			foreach(Option option in Validate(currentNode.options))
				size.x = Mathf.Clamp(textStyle.CalcSize(new GUIContent(option.text)).x + extra + optionIndent, size.x, max);
		}
		if (heightStyle == SizeStyle.StaticPercent)
			size.y = (nodeSize.y/100 * Screen.height) - nodeBorder.y * 2;
		else if (heightStyle == SizeStyle.StaticPixel)
			size.y = nodeSize.y - nodeBorder.y * 2;	
		else if (heightStyle == SizeStyle.DynamicPercent || heightStyle == SizeStyle.DynamicPixel){
			float max = (heightStyle == SizeStyle.DynamicPercent) ? Screen.height * nodeSize.y/100 - nodeBorder.y * 2 : nodeSize.y - nodeBorder.y * 2;
			if (displayName && currentActor != null)
				size.y += textStyle.CalcHeight(new GUIContent(currentActor.name), size.x);
			foreach(Line line in Validate(currentNode.lines))
				size.y += textStyle.CalcHeight(new GUIContent(line.text), size.x - extra - textIndent);
			foreach(Option option in Validate(currentNode.options))
				size.y += textStyle.CalcHeight(new GUIContent(option.text), size.x - extra - optionIndent);
			size.y = (displayImage && currentActor != null && currentActor.image && imageSize > size.y) ? imageSize : size.y;    //dialogue will be at least height of image unless at max
			size.y = Mathf.Clamp(size.y, 0, max);
		}
		return size;
	}
	
	//Gets position of dialogue given user input and size of dialogue
	Vector2 GetPosition(Vector2 size){
		Vector2 pos = new Vector2((Screen.width - size.x) * defaultPosition.x, (Screen.height - size.y) * defaultPosition.y); //default position
		if (currentActor != null && currentActor.positionType == Actor.PositionType.Static){
			if (currentActor.position == TextAnchor.LowerCenter || currentActor.position == TextAnchor.MiddleCenter || currentActor.position == TextAnchor.UpperCenter)
				pos.x = 0.5f * Screen.width - size.x/2;
			else if (currentActor.position == TextAnchor.LowerLeft || currentActor.position == TextAnchor.MiddleLeft || currentActor.position == TextAnchor.UpperLeft)
				pos.x = 0;
			else if (currentActor.position == TextAnchor.LowerRight || currentActor.position == TextAnchor.MiddleRight || currentActor.position == TextAnchor.UpperRight)
				pos.x = Screen.width - size.x;	
			if (currentActor.position == TextAnchor.LowerCenter || currentActor.position == TextAnchor.LowerLeft || currentActor.position == TextAnchor.LowerRight)
				pos.y = Screen.height - size.y;
			else if (currentActor.position == TextAnchor.MiddleCenter || currentActor.position == TextAnchor.MiddleLeft || currentActor.position == TextAnchor.MiddleRight)
				pos.y = Screen.height/2 - size.y/2;	
			else if (currentActor.position == TextAnchor.UpperCenter || currentActor.position == TextAnchor.UpperLeft || currentActor.position == TextAnchor.UpperRight)
				pos.y = 0;	
			
			pos.x += currentActor.offset.x * Screen.width;	
			pos.y += currentActor.offset.y * Screen.height;
		}
		else if (currentActor != null && currentActor.positionType == Actor.PositionType.Dynamic){
			Vector3 currentActorPosition = Vector3.zero;
			if (currentActor != null && currentActor.transform){
				Transform transform = currentActor.transform;
				currentActorPosition = transform.position;
				if (currentActor.autoPosition){
					Bounds bounds = (transform.GetComponent<Renderer>()) ? transform.GetComponent<Renderer>().bounds : new Bounds(transform.position, Vector3.zero);
					foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
						bounds.Encapsulate(renderer.bounds);
					currentActorPosition = bounds.center + transform.up * bounds.extents.y;
				}
				currentActorPosition += transform.right * -currentActor.offset.x + transform.up * currentActor.offset.y + transform.forward * currentActor.offset.z;							
			}
			Vector3 charPos = Camera.main.WorldToScreenPoint(currentActorPosition);
			pos.x = charPos.x - size.x/2;
			pos.y = Screen.height - charPos.y - size.y;
			
			if (!stickToEdges && charPos.z < 0)
				pos = -Vector2.one * 10000; //off the screen
			if (stickToEdges){
				if (charPos.z < 0){
					pos.x = -pos.x;
					pos.y = Screen.height -pos.y;
				}
				pos.x = Mathf.Clamp(pos.x, 0, Screen.width - size.x);
				pos.y = Mathf.Clamp(pos.y, 0, Screen.height - size.y);
			}
		}
		return pos;
	} 
	
	//moves dialogue index to new index and performs ...
	void Next(int nextNodeID){
		Node newNode = dialogue.nodes.Find(x => x.nodeID == nextNodeID);
		selectionIndex = 0;
		if (newNode != null){
			if (newNode.nodeID == 0){
				Next(newNode.output.nextNodeID);
				return;
			}
			foreach(Redirection redirection in Validate(newNode.redirections)){
				if (redirection.automatic){
					Next(redirection.output.nextNodeID);
					return;
				}
			}
			
			currentNode = newNode;
			currentActor = actors.Find(x => x.actorID == currentNode.actorID);

			foreach (Action action in currentNode.actions){ 	//for every action in current node
				if (action.operation == Action.Operation.Add)
					SetKey(action.name,action.number, true);
				else if (action.operation == Action.Operation.Subtract)
					SetKey(action.name,-action.number, true);
				else if (action.operation == Action.Operation.Set)	
					SetKey(action.name,action.number, false);
			}
		}
		else{
			if (nextNodeID != -1){
				if (nextNodeID == 0)
					Debug.Log("No Start Node was found.");
				else{
					if (currentNode != null)
						Debug.Log("Error @ Node '" + currentNode.nodeID + "': A node with the ID '" + nextNodeID + "' was not found.");
					else
						Debug.Log("Error Loading Node: A node with the ID '" + nextNodeID + "' was not found.");
				}
			}
			CloseConversation();
		}
	}
	
	//Validates given list of elements
	List<T> Validate<T>(List<T> oldList){
		List<T> newList = new List<T>();
		for (int i = 0; i < oldList.Count; i++){
			List<Condition> conditions = (oldList[i] as Element).conditions;
			int failed = conditions.Count;
			if (conditions.Count > 0){
				foreach (Condition condition in conditions){
					if (Evaluate(condition))
						failed --;
				}
				if(failed == 0)
					newList.Add(oldList[i]);
			}
			else
				newList.Add(oldList[i]);
		}
		return newList;
	}
	
	//Evaluation used in validation
	bool Evaluate(Condition condition){
		int num1 = GetKey(condition.name);
		int num2 = condition.number;
		switch (condition.operation){
			case Condition.Operation.Equals: return num1 == num2;
			case Condition.Operation.DoesNotEqual: return num1 != num2;
			case Condition.Operation.GreaterOrEquals: return num1 >= num2;
			case Condition.Operation.LessOrEquals: return num1 <= num2;
			case Condition.Operation.GreaterThan: return num1 > num2;
			case Condition.Operation.LessThan: return num1 < num2;
			default: return false;
		}
	}
	
	//Text style used for options and lines
	GUIStyle TextStyle(){
		GUIStyle style = new GUIStyle();
		style.wordWrap = true;
		style.alignment = alignment;
		style.fontSize = fontSize;
		style.font = font;
		style.normal.textColor = fontColor;
		
		Texture2D newTexture = new Texture2D(1, 1);
		newTexture.SetPixel(1,1, new Color(0,0,0,0));
		newTexture.Apply();
		
		style.focused.background = newTexture;
		style.focused.textColor = highlightFontColor;
		return style;
	}
	
	//Gets style used for node and inspector text
	GUIStyle BackgroundStyle(Texture2D texture, Vector2 border){
		GUIStyle style = new GUIStyle();
		style.normal.background = texture;
		style.overflow = new RectOffset((int)border.x, (int)border.x, (int)border.y, (int)border.y);
		return style;	
	}
	
	//returns current number of given key in keys list
	int GetKey(string name){
		int number = 0;
		Key existingKey = Keys.Find(x => x.name == name);
		if(existingKey != null)
			number = existingKey.number;
		return number;
	}
	
	//adds a key to the keys list.
	void SetKey (string name, int number, bool add) {
		Key existingKey = Keys.Find(x => x.name == name); 
		if (!add && existingKey != null)
			existingKey.number = number;
		else if (add && existingKey != null)
			existingKey.number += number;
		else
			Keys.Add(new Key(name, number)); 
	}
}

//a dialogue key
[System.Serializable]
public class Key {
	public string name;
	public int number;
	public Key(string newName, int newNumber){
		name = newName;
		number = newNumber;
	}
}

//an actor
[System.Serializable]
public class Actor {
	public bool show = false;
	public int actorID;
	public string name;
	public Texture2D image;
	public bool imageRight = false;
	public enum PositionType{Static, Dynamic}
	public PositionType positionType = PositionType.Static;
	public TextAnchor position = TextAnchor.LowerCenter;
	public Transform transform = null;
	public bool autoPosition = false; 					//moves node with actor - set to false for better performance
	public Vector3 offset;
}