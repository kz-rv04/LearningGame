using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIScript : MonoBehaviour {

	private bool isLON;
	private bool isRON;
	private bool isUON;
	private bool isDON;
	private float speed;
	private float distance;
	private LinkedList<string> resultList = new LinkedList<string>();

	private object guard = new object();

	void OnGUI() {

		string str = "";
		lock (guard) {
			foreach (var result in resultList) {
				str += result + "\r\n";
			}
		}
		GUI.Label (new Rect(10, 110, 500, 500), str);
	}

	public void setResult(string str) {
		lock (guard) {
			resultList.AddFirst (str);
			if (resultList.Count > 30) {
				resultList.RemoveLast ();
			}
		}
	}
}
