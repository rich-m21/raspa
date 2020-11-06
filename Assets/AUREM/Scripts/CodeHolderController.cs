using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CodeHolderController : MonoBehaviour {
    public GameObject buttonTxt;
    public GameObject buttonCheck;
    public Text code;
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void GenerateRaspable() {
        UI_Manager.instance.UI_GENERATE_SCRATCH_PRESSED(this.gameObject.name);
    }

    public void SetButton(string _c, string _name, bool _enabled) {//Crea los botones
        this.gameObject.name = _name;
        code.text = _c;
        if (_enabled) {
            buttonCheck.SetActive(false);
            buttonTxt.SetActive(true);
        } else {
            Button b = this.GetComponentInChildren<Button>();
            b.interactable = false;
            buttonCheck.SetActive(true);
            buttonTxt.SetActive(false);
        }
    }
}
