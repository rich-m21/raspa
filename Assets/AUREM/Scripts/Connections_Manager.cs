using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Connections_Manager : MonoBehaviour {
	//Manages all outgoing server and external plugin connections
	// IE: Facebook, php scripts, database connections, etc.
	//public static Connections_Manager CM;
	public static Connections_Manager instance;

	public delegate void ServerReturnResponse();

	public static event ServerReturnResponse OnReturnedResponse;
	public static event ServerReturnResponse OnReturnedError;

    public UnityWebRequest webRequest;

    private string serviceUrl = "https://cloud-raspa.com/contest/service.php";
	void Awake() {
		instance = this;
	}

	#region PHP_SCRIPT_FUNCTIONS

	public WWW GET(string url) {

        UI_Manager.instance.networking = true;
		WWW www = new WWW(url);
		StartCoroutine(waitForRequest(www));
		return www;
	}

    private void Start()
    {
        
    }
    public WWW POST(string url, Dictionary<string,string> post,string session) {
        UI_Manager.instance.networking = true;
//		wwwStart = true;
		WWWForm form = new WWWForm();
        Dictionary<string, string> connHeaders = new Dictionary<string, string>();
		foreach (KeyValuePair<String,String> post_arg in post) {
			form.AddField(post_arg.Key, post_arg.Value);
		}
        if(!session.Equals("0")){
            connHeaders = new Dictionary<string, string>();
            connHeaders.Add("Set-Cookie", session);
        }
        byte[] data = form.data;
        WWW www = null;
        if(connHeaders.Count == 0){
            www = new WWW(url, form);
        }else{
            www = new WWW(url, data, connHeaders);
        }
		StartCoroutine(waitForRequest(www));
		return www;
	}

    public void POST(Dictionary<string, string> post){
        UI_Manager.instance.networking = true;
        //      wwwStart = true;
        WWWForm form = new WWWForm();
        //Dictionary<string, string> connHeaders = new Dictionary<string, string>();
        foreach (KeyValuePair<String, String> post_arg in post) {
            form.AddField(post_arg.Key, post_arg.Value);
        }
        byte[] data = form.data;
        StartCoroutine(waitForRequest(data));
    }


    private IEnumerator waitForRequest(byte[] post) {
        UnityWebRequest webRequest = UnityWebRequest.Post(serviceUrl, string.Empty);
        UploadHandlerRaw uH = new UploadHandlerRaw(post);

        webRequest.uploadHandler = uH;
        webRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        webRequest.SetRequestHeader("Cookie", PlayerPrefs.GetString("Cookie", "0"));
        Debug.Log(webRequest.GetRequestHeader("Cookie"));
        yield return webRequest.SendWebRequest();

        if (String.IsNullOrEmpty(webRequest.error)) {
            foreach (KeyValuePair<String, String> head in webRequest.GetResponseHeaders()) {
                Debug.Log(head.Key + ": " + head.Value);
                if(head.Key.Equals("Set-Cookie")){
                    PlayerPrefs.SetString("Cookie", head.Value);
                }
            }
            Debug.Log(webRequest.downloadHandler.text);
            this.webRequest = webRequest;
            OnReturnedResponse();
        } else {
            Debug.Log(webRequest.downloadHandler.text);
            this.webRequest = webRequest;
            OnReturnedError();
        }
    }


	private IEnumerator waitForRequest(WWW www) {
		yield return www;
        if (string.IsNullOrEmpty(www.error)) {
            if (OnReturnedResponse != null) {
                AuremUtils.Log("CONNECTION WWW: " + www.text);
                if(www.responseHeaders.Count > 8){
                    PlayerPrefs.SetString("Set-Cookie",www.responseHeaders["Set-Cookie"]);
                }
                AuremUtils.Log("CONNECTION WWW: " + www.responseHeaders);
                OnReturnedResponse();
            }
		}
		else {
			if (App_Manager.instance.debugEnabled)
				Debug.Log("WWW Error: " + www.error);
            AuremUtils.Log("CONNECTION WWW: " + www.error);
			OnReturnedError();

//			wwwStart = false;
		}
	}

	/*void Awake(){
		if (CM != null) {
			GameObject.Destroy (CM);	
		} else {
			CM = this;
		}

		DontDestroyOnLoad (this);
	}*/

	#endregion //PHP_SCRIPT_FUNCTIONS
}
