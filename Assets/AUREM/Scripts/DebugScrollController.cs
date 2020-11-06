using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DebugScrollController : MonoBehaviour {
	public static DebugScrollController instance;
	// Use this for initialization
	public Text debugText;
	public GameObject openButton;

	void Start () {
		if(instance == null)
			instance = this;
		if(App_Manager.instance.debugEnabled){
			openButton.SetActive(true);
		}
		Application.logMessageReceivedThreaded += HandleLog;
		this.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ClearLog(){
		debugText.text = "";
	}

    private void HandleLog(string logString, string stackTrace, LogType type){
			string tmp = debugText.text;
			tmp += "=====================================\n"+logString;
			debugText.text = tmp;
    }
}
