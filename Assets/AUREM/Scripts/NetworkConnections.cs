using System.Collections;
using UnityEngine;
using System;
using UnityEngine.Networking;

public class NetworkConnections : MonoBehaviour {

    public static NetworkConnections instance;


    public event Action<LoginResponseData> OnLoginReturnResponse;
    public event Action<ErrorResponseData> OnLoginReturnError;
    public event Action<PrizeDrawResponseData> OnDrawPrizeReturnResponse;
    public event Action<ErrorResponseData> OnDrawPrizeReturnError;
    public event Action<ClaimPrizeResponseData> OnClaimPrizeReturnResponse;
    public event Action<ErrorResponseData> OnClaimPrizeReturnError;
    public event Action<LogoutResponseData> OnLogoutReturnResponse;
    public event Action<ErrorResponseData> OnLogoutReturnError;
    public string responseText = "";
    public string errorText = "";
    public float timeOut = 1;
    public bool isCalling = false;

    [SerializeField]
    private string loginUrl = "https://adminraspa.com/endP/login";
    [SerializeField]
    private string drawUrl = "https://adminraspa.com/endP/drawPrize";
    [SerializeField]
    private string claimUrl = "https://adminraspa.com/endP/claimPrize";
    [SerializeField]
    private string logoutUrl = "https://adminraspa.com/endP/logout";
    // Use this for initialization
    void Start() {
        if (instance == null)
            instance = this;
    }

    public void Login(LoginRequestData body) {
        StartCoroutine(RequestLogin(body));
    }

    private IEnumerator RequestLogin(LoginRequestData body) {
        isCalling = true;
        Debug.Log(body);
        string json = JsonUtility.ToJson(body);
        UnityWebRequest webRequest = UnityWebRequest.Post(loginUrl, string.Empty);
        UploadHandlerRaw uH = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        webRequest.uploadHandler = uH;
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Accept", "application/json");
        yield return webRequest.SendWebRequest();
        isCalling = false;
        App_Manager.instance.CancelNetworkTimeout();
        if (String.IsNullOrEmpty(webRequest.error)) {
            Debug.Log(webRequest.downloadHandler.text);
            LoginResponseData response = JsonUtility.FromJson<LoginResponseData>(webRequest.downloadHandler.text);
            if (OnLoginReturnResponse != null)
                OnLoginReturnResponse(response);

        } else {
            Debug.Log(webRequest.error);
            Debug.Log(webRequest.downloadHandler.text);
            ErrorResponseData error = JsonUtility.FromJson<ErrorResponseData>(webRequest.downloadHandler.text);
            if (OnLoginReturnError != null)
                OnLoginReturnError(error);
        }
    }

    public void DrawPrize(DrawPrizeRequestData body) {
        StartCoroutine(RequestDraw(JsonUtility.ToJson(body)));
    }

    private IEnumerator RequestDraw(string body) {
        isCalling = true;
        UnityWebRequest webRequest = UnityWebRequest.Post(drawUrl, string.Empty);
        UploadHandlerRaw uH = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        webRequest.uploadHandler = uH;
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Accept", "application/json");
        yield return webRequest.SendWebRequest();
        App_Manager.instance.CancelNetworkTimeout();
        isCalling = false;
        if (String.IsNullOrEmpty(webRequest.error)) {
            PrizeDrawResponseData response = JsonUtility.FromJson<PrizeDrawResponseData>(webRequest.downloadHandler.text);
            if (OnDrawPrizeReturnResponse != null)
                OnDrawPrizeReturnResponse(response);
        } else {
            ErrorResponseData error = JsonUtility.FromJson<ErrorResponseData>(webRequest.downloadHandler.text);
            if (OnDrawPrizeReturnError != null)
                OnDrawPrizeReturnError(error);
        }
    }

    public void ClaimPrize(ClaimPrizeRequestData body) {
        StartCoroutine(RequestClaim(JsonUtility.ToJson(body)));
    }

    private IEnumerator RequestClaim(string body) {
        isCalling = true;
        UnityWebRequest webRequest = UnityWebRequest.Post(claimUrl, string.Empty);
        UploadHandlerRaw uH = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        webRequest.uploadHandler = uH;
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Accept", "application/json");
        yield return webRequest.SendWebRequest();
        App_Manager.instance.CancelNetworkTimeout();
        isCalling = false;
        if (String.IsNullOrEmpty(webRequest.error)) {
            ClaimPrizeResponseData response = JsonUtility.FromJson<ClaimPrizeResponseData>(webRequest.downloadHandler.text);
            if (OnClaimPrizeReturnResponse != null)
                OnClaimPrizeReturnResponse(response);
        } else {
            ErrorResponseData error = JsonUtility.FromJson<ErrorResponseData>(webRequest.downloadHandler.text);
            if (OnClaimPrizeReturnError != null)
                OnClaimPrizeReturnError(error);
        }
    }

    public void Logout(LogoutRequestData body) {
        StartCoroutine(RequestLogout(JsonUtility.ToJson(body)));
    }

    private IEnumerator RequestLogout(string body) {
        isCalling = true;
        UnityWebRequest webRequest = UnityWebRequest.Post(logoutUrl, string.Empty);
        UploadHandlerRaw uH = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        webRequest.uploadHandler = uH;
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Accept", "application/json");
        yield return webRequest.SendWebRequest();
        App_Manager.instance.CancelNetworkTimeout();
        isCalling = false;
        if (String.IsNullOrEmpty(webRequest.error)) {
            LogoutResponseData response = JsonUtility.FromJson<LogoutResponseData>(webRequest.downloadHandler.text);
            if (OnLogoutReturnResponse != null)
                OnLogoutReturnResponse(response);
        } else {
            ErrorResponseData error = JsonUtility.FromJson<ErrorResponseData>(webRequest.downloadHandler.text);
            if (OnLogoutReturnError != null)
                OnLogoutReturnError(error);
        }
    }



    //private IEnumerator A110Helper(WWWForm fData) {
    //    UnityWebRequest webRequest = UnityWebRequest.Post(serviceUrl + "?action=A110", fData);
    //    if (!PlayerPrefs.GetString("cookie", "0").Equals("0")) {
    //        webRequest.SetRequestHeader("cookie", PlayerPrefs.GetString("cookie", "0"));
    //        webRequest.SetRequestHeader("xcsrf", App_Manager.instance.xcsrf.Replace("\r",""));
    //        webRequest.SetRequestHeader("tokenkey", App_Manager.instance.tokenkey.Replace("\r", ""));
    //    }

    //    yield return webRequest.SendWebRequest();

    //    if (String.IsNullOrEmpty(webRequest.error)) {
    //        responseText = webRequest.downloadHandler.text;
    //        if (OnReturnedResponse != null)
    //            OnReturnedResponse();
    //    } else {
    //        errorText = webRequest.error;
    //        OnReturnedError();
    //    }
    //}

    //public void POST(Dictionary<string,string> _params, string action){
    //    UI_Manager.instance.network_panel.SetActive(true);
    //    responseText = "";
    //    errorText = "";
    //    HTTPRequest request = new HTTPRequest(new Uri(serviceUrl+"?action="+action), HTTPMethods.Post, OnRequestFinished);
    //    request.IsCookiesEnabled = true;
    //    if(!PlayerPrefs.GetString("cookie","0").Equals("0")){
    //        request.AddHeader("cookie",PlayerPrefs.GetString("cookie", "0"));
    //    }
    //    foreach (KeyValuePair<String, String> post_arg in _params) {
    //        request.AddField(post_arg.Key, post_arg.Value);
    //    }
    //    request.Send();
    //}


    //public void LOGOUT(WWWForm _params) {
    //    UI_Manager.instance.network_panel.SetActive(true);
    //    responseText = "";
    //    errorText = "";
    //    StartCoroutine(LogoutHelper(_params));
    //    //HTTPRequest request = new HTTPRequest(new Uri(serviceUrl+"?action=A103"), HTTPMethods.Post, OnRequestFinished);
    //    //request.IsCookiesEnabled = true;
    //    //if (!PlayerPrefs.GetString("cookie", "0").Equals("0")) {
    //    //    request.AddHeader("cookie", PlayerPrefs.GetString("cookie", "0"));
    //    //    request.AddHeader("dvar", App_Manager.instance.d);
    //    //    request.AddHeader("jsession", App_Manager.instance.jcookie);
    //    //    request.AddHeader("tokenkey", App_Manager.instance.tokenkey.Replace("\r", ""));
    //    //    request.AddHeader("xcsrf", App_Manager.instance.xcsrf.Replace("\r", ""));
    //    //}
    //    //foreach (KeyValuePair<String, String> post_arg in _params) {
    //    //    request.AddField(post_arg.Key, post_arg.Value);
    //    //}
    //    //request.Send();
    //}

    //private IEnumerator LogoutHelper(WWWForm fData) {
    //    UnityWebRequest webRequest = UnityWebRequest.Post(serviceUrl + "?action=A103", fData);
    //    //string json = JsonUtility.ToJson(a110Data);
    //    //UploadHandlerRaw uH = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
    //    //webRequest.uploadHandler = uH;
    //    if (!PlayerPrefs.GetString("cookie", "0").Equals("0")) {
    //        webRequest.SetRequestHeader("cookie", PlayerPrefs.GetString("cookie", "0"));
    //        webRequest.SetRequestHeader("xcsrf", App_Manager.instance.xcsrf.Replace("\r", ""));
    //        webRequest.SetRequestHeader("tokenkey", App_Manager.instance.tokenkey.Replace("\r", ""));
    //        webRequest.SetRequestHeader("jsession", App_Manager.instance.jcookie.Replace("\r", ""));
    //        webRequest.SetRequestHeader("dvar", App_Manager.instance.d);
    //    }

    //    yield return webRequest.SendWebRequest();

    //    if (String.IsNullOrEmpty(webRequest.error)) {
    //        responseText = webRequest.downloadHandler.text;
    //        if (OnReturnedResponse != null)
    //            OnReturnedResponse();
    //    } else {
    //        errorText = webRequest.error;
    //        OnReturnedError();
    //    }
    //}

    //public void A110(WWWForm fData) {
    //    Debug.Log("DRAW ATTEMPT");
    //    UI_Manager.instance.network_panel.SetActive(true);
    //    responseText = "";
    //    errorText = "";
    //    StartCoroutine(A110Helper(fData));
    //    //Uri cloud = new Uri(serviceUrl+"?action=A110");
    //    //HTTPRequest A110request = new HTTPRequest(cloud, HTTPMethods.Post, OnRequestFinished);
    //    //A110request.IsCookiesEnabled = true;
    //    //if (!PlayerPrefs.GetString("cookie", "0").Equals("0")) {
    //    //    A110request.AddHeader("cookie", PlayerPrefs.GetString("cookie", "0"));
    //    //    A110request.AddHeader("xcsrf", App_Manager.instance.xcsrf);
    //    //    A110request.AddHeader("tokenkey", App_Manager.instance.tokenkey);
    //    //}
    //    //foreach (KeyValuePair<String, String> post_arg in _params) {
    //    //    if(post_arg.Key != null)
    //    //        A110request.AddField(post_arg.Key, post_arg.Value);
    //    //}
    //    //A110request.Send();
    //}

    //void OnRequestFinished(HTTPRequest _request, HTTPResponse _response) {
    //    UI_Manager.instance.network_panel.SetActive(false);
    //    try {
    //        Debug.Log("Request Finished! Text received: " + _response.DataAsText);
    //        if (_response.IsSuccess) {
    //            Dictionary<string, List<string>> d = _response.Headers;
    //            foreach (KeyValuePair<String, List<String>> p in d) {
    //                Debug.Log(p.Key);
    //                if (p.Key.Equals("set-cookie")) {
    //                    string[] g = p.Value.ToArray();
    //                    foreach (string cuk in g) {
    //                        Debug.Log(cuk);
    //                        PlayerPrefs.SetString("cookie", cuk);
    //                    }
    //                }
    //            }

    //            responseText = _response.DataAsText;
    //            OnReturnedResponse();
    //        } else {
    //            errorText = _response.DataAsText;
    //            OnReturnedError();
    //        }
    //    }catch(Exception e){
    //        Debug.Log(e.Message);
    //    }
    //}



}
