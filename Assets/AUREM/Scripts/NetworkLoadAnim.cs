using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkLoadAnim : MonoBehaviour {

	public GameObject wheel;
	private bool anim;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(anim){
			Vector3 rot = wheel.transform.localRotation.eulerAngles - new Vector3(0f,0f,20f);
			iTween.RotateUpdate(wheel,iTween.Hash("rotation", rot, "time", Time.deltaTime));
		}
	}

	void OnEnable(){
		anim = true;
	}

	void OnDisable(){
		anim = false;
	}
}
