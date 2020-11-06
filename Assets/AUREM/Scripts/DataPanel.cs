using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AdvancedInputFieldPlugin;
public class DataPanel : MonoBehaviour {
    public AdvancedInputField email;
    public AdvancedInputField phone;
    private void OnEnable()
    {
        email.Text = PlayerPrefs.GetString("email", "");
        phone.Text = PlayerPrefs.GetString("phone", "");
    }
}
