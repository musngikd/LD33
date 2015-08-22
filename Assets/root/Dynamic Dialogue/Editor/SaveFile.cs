using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveFile : ScriptableObject {
	[HideInInspector]
	public List<Node> nodes = new List<Node>();
	[HideInInspector]
	public int autoNumNode = 1;
	[HideInInspector]
	public Vector2 gridPosition = new Vector2(0,0); 
	[HideInInspector]
	public float zoom = 1.0f;
}