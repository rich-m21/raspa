using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrizePanelControl : MonoBehaviour {

	public GameObject normal;
	public GameObject win;
	public GameObject lose;
	public GameObject hider;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnEnable(){
		hider.SetActive(false);
		normal.SetActive(true);
		win.SetActive(false);
		lose.SetActive(false);
	}
}
