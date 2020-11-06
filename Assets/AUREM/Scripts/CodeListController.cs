using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodeListController : MonoBehaviour {

    public GameObject ContentPanel;
    public GameObject codes_prefab;
    public GameObject noCodes_prefab;

    private bool switchEnabled = true;

    private CodeButton[] codes;
    private GameObject[] codesGO;

    // Use this for initialization//Version01
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void OnSwitchButtonEvent()
    {
        switchEnabled = !switchEnabled;
        GameObject.Find("/main_canvas/scene/main_panel/switch_button/background").GetComponent<Animator>().SetBool("enabled", switchEnabled);
        
        if (!switchEnabled)
        {
            for(int i = 0; i < codes.Length; i++)
            {
                if (!codes[i].enabled) codesGO[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < codes.Length; i++)
            {
                codesGO[i].SetActive(true);
            }
        }
        
    }

    private void OnEnable() {//Crea la lista

        if (!PlayerPrefs.HasKey("Logged"))
        {
            PlayerPrefs.SetInt("Logged", 1);
            switchEnabled = true;
        }

        codes = App_Manager.instance.codes.ToArray();
        codesGO = new GameObject[codes.Length];

        if (codes != null) {
            for (int i = 0; i < codes.Length; i++) {
                GameObject code = Instantiate(codes_prefab, ContentPanel.transform);
                CodeHolderController c = code.GetComponent<CodeHolderController>();
                c.SetButton( codes[i].refId, codes[i].code , codes[i].enabled );
                codesGO[i] = code;
            }
        } else{
            GameObject no_code = Instantiate(noCodes_prefab, ContentPanel.transform);
        }
        switchEnabled = !switchEnabled;
        OnSwitchButtonEvent();

    }

    private void OnDisable() {//Elimina la lista
        CodeHolderController[] codes = ContentPanel.GetComponentsInChildren<CodeHolderController>();
        if (codes != null) {
            for (int i = 0; i < codes.Length; i++) {
                Destroy(codes[i].gameObject);
            }
        }
    }
}
