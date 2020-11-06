using UnityEngine;
using System.Collections;
using System.Text;

public static class AuremUtils  {

	public static string RemoveControlCharacters(string inString) {
		if (inString == null)
			return null;
		StringBuilder newString = new StringBuilder();
		char ch;
		for (int i = 0; i < inString.Length; i++) {
			ch = inString[i];
			if (!char.IsControl(ch)) {
				newString.Append(ch);
			}
		}
		return newString.ToString();
	}

	public static void Log(string s){
		if(App_Manager.instance.debugEnabled){
			Debug.Log(s);
		}
	}

    public static void Log(Vector3 s) {
        if (App_Manager.instance.debugEnabled) {
            Debug.Log(s);
        }
    }
    public static void Log(Vector2 s) {
        if (App_Manager.instance.debugEnabled) {
            Debug.Log(s);
        }
    }
}
