using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class App_Manager : MonoBehaviour
{
    public static App_Manager instance;
    private bool kb_out = false;
    private bool inputSelected = false;
    public bool authing = true;
    public string selectedInput = "";

    public bool sessionActive;
    public bool debugEnabled;


    public bool prize_won = false;
    public string session_user;
    public DateTime webActionTime;

    public string error_msg;
    public string won_msg;
    public bool data_set;

    public string sent_code;

    private Vector3 key_init_pos;
    public List<CodeButton> codes;
    private TouchScreenKeyboard tk;

    // Use this for initialization
    void Start()
    {
        codes = new List<CodeButton>();
        //PlayerPrefs.SetString("prizename","Galaxy S8");
        key_init_pos = UI_Manager.instance.mainscene.transform.position;
        //HideKeyboard();
        session_user = PlayerPrefs.GetString("user", "0");
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            DateTime newDate = DateTime.Now;
            DateTime savedDate;
            DateTime.TryParse(PlayerPrefs.GetString("sessionTime", ""), out savedDate);
            TimeSpan timeDiff = newDate - savedDate;
            if (timeDiff.Minutes > 5 && !string.IsNullOrEmpty(PlayerPrefs.GetString("auth", "")))
            {
                CancelInvoke("SessionTimeClose");
                SessionTimeClose();
                UI_Manager.instance.ShowErrorDialog("Sesión cerrada por inactividad.");
            }
        }
    }

    void Awake()
    {
        instance = this;
    }

    public void RenewSessionTimeLimit()
    {
        CancelInvoke("SessionTimeClose");
        webActionTime = DateTime.Now;
        PlayerPrefs.SetString("sessionTime", webActionTime.ToString());
        Invoke("SessionTimeClose", 300f);
    }

    public void SessionTimeClose()
    {
        ReleaseUserAuth();
        UI_Manager.instance.ShowLoginPanel();
    }

    public void SetUserAuth(string _user)
    {
        session_user = _user;
        PlayerPrefs.SetString("user", session_user);
        PlayerPrefs.SetString("sessionTime", webActionTime.ToString());
    }

    public void ReleaseUserAuth()
    {
        session_user = "";
        PlayerPrefs.DeleteAll();
    }

    public void CrossCodeFromList()
    {
        CodeButton found = codes.Find((obj) => obj.code == sent_code);
        if (found != null)
            found.ChangeEnabled(false);
    }



    #region NETWORK_FUNCTIONS
    public void CancelNetworkTimeout(){
        Debug.Log("CANCELTIMEOUT");
        
        CancelInvoke("NetWorkTimeoutError");
    }
    public void NetWorkTimeoutError()
    {
        Debug.Log("TIMEOUT");
        
        UI_Manager.instance.ShowErrorDialog("Tiempo de espera de red ha sobrepasado, intenta mas tarde.");
    }
    public void A101(string m_username, string password)
    {
        LoginRequestData logParams = new LoginRequestData
        {
            mUniqueId = SystemInfo.deviceUniqueIdentifier,
            osVersion = SystemInfo.operatingSystem,
            deviceModel = SystemInfo.deviceModel,
            password = password,
            mUsername = m_username
        };
        Invoke("NetWorkTimeoutError", NetworkConnections.instance.timeOut);
        NetworkConnections.instance.Login(logParams);
    }

    public void A111(string m_phone, string m_email, string prizename)
    {
        AuremUtils.Log("USER: " + session_user);
        ClaimPrizeRequestData cp = new ClaimPrizeRequestData
        {
            mUsername = session_user,
            mEmail = m_email,
            mPhone = m_phone,
            prizeName = prizename,
            auth = PlayerPrefs.GetString("auth", "0")
        };
        NetworkConnections.instance.ClaimPrize(cp);
    }

    public void A110(string b_code)
    {

        DrawPrizeRequestData dp = new DrawPrizeRequestData
        {
            auth = PlayerPrefs.GetString("auth", "0"),
            bankCode = b_code,
            mUsername = session_user
        };

        sent_code = b_code;
        NetworkConnections.instance.DrawPrize(dp);
    }

    public void A103()
    {
        LogoutRequestData ld = new LogoutRequestData
        {
            auth = PlayerPrefs.GetString("auth", "0"),
            mUsername = session_user
        };
        NetworkConnections.instance.Logout(ld);
    }

    private bool CheckInternetConnection()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
            return false;
        else
            return true;

    }

    #endregion //NETWORK_FUNCTIONS
}
