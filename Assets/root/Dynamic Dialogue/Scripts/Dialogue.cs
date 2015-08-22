using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class Dialogue : MonoBehaviour {
	public List<Node> nodes = new List<Node>();
	public int autoNumNode = 1;
	public Vector2 gridPosition = new Vector2(0,0); 
	public float zoom = 1.0f;
}

[System.Serializable]
public class Node {
	public int nodeID;
	public int actorID;
	public List<Line> lines = new List<Line>();
	public List<Option> options = new List<Option>();
	public List<Redirection> redirections = new List<Redirection>();
	public List<Action> actions = new List<Action>();
	public Output input = new Output(Color.blue);
	public Output output = new Output(Color.blue);
	public Vector2 position;
	public Rect rect;
}

//a dialogue line
[System.Serializable]
public class Line : Element  {
	public string text = "";
	public Line(){}
	public Line(Line line){
		text = line.text;
	}
}

//a dialogue option
[System.Serializable]
public class Option : Element  {
	public string text = "";
	public Output output = new Output(Color.red);
	public Option(){}
	public Option(Option option){
		text = option.text;
		output = new Output(option.output);
	}	
}

//a redirection
[System.Serializable]
public class Redirection : Element {
	public bool automatic = false;
	public Output output = new Output(Color.green);	
	public Redirection(){}
	public Redirection(Redirection redirection){
		automatic = redirection.automatic;
		output = new Output(redirection.output);
	}
}

//a key action such as adding or removing a key
[System.Serializable]
public class Action : Element {
	public string name = "X";
	public enum Operation {Add, Subtract, Set}
	public Operation operation;
	public int number = 0;
	public Action(){}
	public Action(Action action){
		name = action.name;
		operation = action.operation;
		number = action.number;
	}
}

[System.Serializable]
public class Element {
	public List<Condition> conditions = new List<Condition>();
	public Rect rect;
}

//a key action such as adding or removing a key
[System.Serializable]
public class Output {
	public Rect rect;
	public Color color;
	public int nextNodeID = -1;
	public Output(Color newColor){
		color = newColor;
	}
	public Output(Output output){
		rect = output.rect;
		color = output.color;
		nextNodeID = output.nextNodeID;
	}
}

[System.Serializable]
public class Condition {
	public string name;
	public enum Operation {Equals, DoesNotEqual, GreaterOrEquals, LessOrEquals, LessThan, GreaterThan}
	public Operation operation;
	public int number;
	public bool show = false;
}