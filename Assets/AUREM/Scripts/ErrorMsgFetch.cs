using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorMsgFetch : MonoBehaviour {
	public Text error_msg;
	// Use this for initialization
	void Start () {
		
	}

	void OnEnable(){
		error_msg.text = App_Manager.instance.error_msg;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
