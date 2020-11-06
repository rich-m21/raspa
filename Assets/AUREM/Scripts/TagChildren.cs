using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagChildren : MonoBehaviour {
    public string _tag;
	// Use this for initialization
	void Start () {
        TagAllChildren();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TagAllChildren(){
        Transform[] c = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < c.Length;i++){
            c[i].gameObject.tag = _tag;
        }
    }
}
