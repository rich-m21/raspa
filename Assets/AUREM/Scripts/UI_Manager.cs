using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Mail;
using System;
using System.IO;
using System.Text.RegularExpressions;
using AdvancedInputFieldPlugin;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;
    #region UI_PANELS_VARIABLES
    public RectTransform mainscene;
    public RectTransform[] panels;
    public RectTransform kbDisplace;
    public RectTransform scrnDisp;
    private Vector3 initKb;
    #endregion
    [Space(20)]
    private int tus_credenciales = 0;
    public Sprite[] categories;
    public SpriteRenderer prize_img;
    private string prizename;
    public bool networking;

    public GameObject network_panel;
    public GameObject notify_panel;

    // Use this for initialization
    void Start()
    {
        initKb = mainscene.transform.position;
        InitUI();
        ShowLoginPanel();
        //		CheckAuthCode();
    }

    // Update is called once per frame
    private bool veredict = false;
    void Update()
    {
        CheckForKeyboard();
        if (prog.Progreso > 60f)
        {
            if (App_Manager.instance.prize_won)
            {
                AuremUtils.Log("GANASTE");
                WinPrizeShow();
            }
            else
            {
                LosePrizeShow();
                AuremUtils.Log("PERDISTE");
            }
            veredict = true;
        }


    }

    void Awake()
    {
        instance = this;
    }

    public void CheckForKeyboard()
    {
        //Debug.Log("Difference: " + UI_Manager.instance.mainscene.transform.position + "-" + UI_Manager.instance.scrnDisp.position);
        if (pass_input.Selected || user_input.Selected || email_input.Selected || phone_input.Selected || pass_view_input.Selected || pass_input.Selected)
        {
            ShowKeyboard();
            //Debug.Log("Show");
        }
        else
        {
            HideKeyboard();
            //Debug.Log("Hide");
        }
        //if (EventSystem.current.currentSelectedGameObject.name.Contains("InputField")) {
        //    ShowKeyboard();
        //} else {
        //    HideKeyboard();
        //}
    }

    public void InitUI()
    {
        HideScratchCard();
        ErrorLoginUIHide();
        ShowLoadingPanel();
    }


    public void HideKeyboard()
    {

        iTween.MoveTo(UI_Manager.instance.mainscene.gameObject, iTween.Hash("position", initKb, "time", 1f));


    }

    public void ShowKeyboard()
    {

        //if (UI_Manager.instance.mainscene.transform.position != UI_Manager.instance.scrnDisp.position) {
        iTween.MoveTo(UI_Manager.instance.mainscene.gameObject, iTween.Hash("position", UI_Manager.instance.scrnDisp.position, "time", 1f));
        //}

    }


    public void HideScratchCard()
    {
        sc.gameObject.SetActive(false);
    }

    public void ShowScratchCard()
    {
        sc.gameObject.SetActive(true);
    }

    #region UI_PANEL_TRANSITIONS

    public void GoToMainPanel()
    {
        //		App_Manager.instance.sdui.Progreso = 0;
        //		App_Manager.instance.s_s.ResetScratchCard();
        //		App_Manager.instance.scratchCard.SetActive(false);
        //		back_button.SetActive(false);
        //		ShowPanel("main");
        //		prize_txt.text = "";
    }

    private void PickPanel(int _i)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (_i == i)
            {
                panels[i].gameObject.SetActive(true);
            }
            else
            {
                panels[i].gameObject.SetActive(false);
            }
        }
    }

    public void ShowLoadingPanel()
    {
        PickPanel(0);
    }

    public void ShowGoBackPanel()
    {
        PickPanel(6);
    }

    public void ShowMainPanel()
    {
        PickPanel(3);
    }

    public void ShowScratchPanel()
    {
        PickPanel(4);
    }

    public void ShowDataPanel()
    {
        PickPanel(5);
    }

    public void ShowLoginPanel()
    {
        PickPanel(1);
    }

    public void ShowRulesPanel()
    {
        PickPanel(2);
    }

    #endregion //UI_PANEL_TRANSITIONS
    //--------------------------------------------------------------------------------- LOGIN PANEL
    #region UI_LOGIN_PANEL_VARS
    public AdvancedInputField user_input;
    public AdvancedInputField pass_input;
    public AdvancedInputField pass_view_input;
    public bool password_hidden = true;

    public RectTransform error_holder;
    public Text err_txt;

    public RectTransform log_button_holder;
    #endregion //UI_LOGIN_PANEL_VARS

    #region UI_LOGIN_PANEL_FUNCTIONS

    public void ChangePassToNormal()
    {
        pass_view_input.Text = pass_input.Text;
        pass_view_input.gameObject.SetActive(true);
        password_hidden = false;
    }

    public void ChangeNormalToPass()
    {
        pass_input.Text = pass_view_input.Text;
        pass_view_input.gameObject.SetActive(false);
        password_hidden = true;
    }

    public void ErrorLoginUIShow()
    {
        error_holder.gameObject.SetActive(true);
        log_button_holder.gameObject.SetActive(false);
        if (!GetPassString().Equals("")) { SetPassString(""); }
        if (!user_input.Text.Equals("")) { user_input.Text = ""; }
        err_txt.text = App_Manager.instance.error_msg;
    }

    public void ErrorLoginUIHide()
    {
        error_holder.gameObject.SetActive(false);
        log_button_holder.gameObject.SetActive(true);
        App_Manager.instance.error_msg = "";
    }

    #endregion //UI_LOGIN_PANEL_FUNCTIONS

    //--------------------------------------------------------------------------------- MAIN PANEL
    [Space(20)]
    #region UI_MAIN_PANEL_VARS
    // public AdvancedInputField code_input;


    #endregion //UI_MAIN_PANEL_VARS

    #region UI_MAIN_PANEL_FUNCTIONS

    #endregion //UI_MAIN_PANEL_FUNCTIONS

    //--------------------------------------------------------------------------------- SCRATCH PANEL
    [Space(20)]
    #region UI_SCRATCH_PANEL_VARS
    public ScratchCard scrchc;
    public GameObject sc;
    public ScratchDemoUI prog;

    public RectTransform normal_s;
    public RectTransform win_s;
    public RectTransform lose_s;

    //public TextMesh sc_msg;

    #endregion //UI_SCRATCH_PANEL_VARS

    #region UI_SCRATCH_PANEL_FUNCTIONS

    public void ResetPrizeShow()
    {
        normal_s.gameObject.SetActive(true);
        win_s.gameObject.SetActive(false);
        lose_s.gameObject.SetActive(false);
    }

    public void WinPrizeShow()
    {
        normal_s.gameObject.SetActive(false);
        win_s.gameObject.SetActive(true);
        lose_s.gameObject.SetActive(false);
    }

    public void LosePrizeShow()
    {
        normal_s.gameObject.SetActive(false);
        win_s.gameObject.SetActive(false);
        lose_s.gameObject.SetActive(true);
    }

    #endregion //UI_SCRATCH_PANEL_FUNCTIONS

    //--------------------------------------------------------------------------------- DATA PANEL

    #region UI_DATA_PANEL_VARS

    public AdvancedInputField email_input;

    public AdvancedInputField phone_input;



    //public Text data_error;

    #endregion //UI_DATA_PANEL_VARS

    #region UI_BUTTONS_PRESSED
    public void UI_RULES_BUTTON_PRESSED()
    {
        ShowRulesPanel();
    }

    public void UI_RULES_TO_LOGIN_PRESSED()
    {
        ShowLoginPanel();
    }

    public void UI_CLAIM_PRIZE_BUTTON_PRESSED()
    {
        veredict = true;
        App_Manager.instance.prize_won = false;
        scrchc.ResetScratchCard();
        prog.Progreso = 0f;
        sc.SetActive(false);
        ShowDataPanel();
    }

    private bool email_ok;
    private bool phone_ok;


    public void UI_ENTER_DATA_BUTTON_PRESSED()
    {
        if (networking)
            return;

        if (email_input.Text.Equals("") || phone_input.Text.Equals(""))
        {
            error_holder.gameObject.SetActive(true);
            err_txt.text = "Recueda llenar todos los datos";
        }
        else
        {
            VerifyEmailFormat();
            VerifyPhoneFormat();
            if (email_ok && phone_ok)
            {

                network_panel.SetActive(true);
                NetworkConnections.instance.OnClaimPrizeReturnResponse += A111Response;
                NetworkConnections.instance.OnClaimPrizeReturnError += ServerErrorResponses;
                PlayerPrefs.SetString("email", email_input.Text);
                PlayerPrefs.SetString("phone", phone_input.Text);
                App_Manager.instance.A111(phone_input.Text, email_input.Text, PlayerPrefs.GetString("prizename", ""));

                email_input.Text = "";

                email_input.TextRenderer.color = new Color32(50, 50, 50, 255);
                phone_input.Text = "";

                phone_input.TextRenderer.color = new Color32(50, 50, 50, 255);
                App_Manager.instance.authing = false;
            }
            else
            {
                error_holder.gameObject.SetActive(true);
                err_txt.text = "Arregla formato\n de datos en rojo";
            }
        }
    }

    public void UI_GO_BACK_PRESSED()
    {
        //App_Manager.instance.HideKeyboard();
        ShowMainPanel();
        App_Manager.instance.authing = false;
    }

    public void RemoveWhitespace()
    {
        email_input.Text = email_input.Text.Replace(" ", String.Empty);
    }

    public void VerifyEmailFormat()
    {

        if (Regex.IsMatch(email_input.Text, "^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-\\w]*[0-9a-z]*\\.)+[a-z0-9][\\-a-z0-9]{0,22}[a-z0-9]))$"))
        {
            email_ok = true;
            email_input.TextRenderer.color = new Color32(50, 50, 50, 255);
        }
        else
        {
            // email_input.textComponent.color = Color.red;
            email_input.TextRenderer.color = Color.red;
            email_ok = false;
        }
    }

    public void VerifyPhoneFormat()
    {
        if (Regex.IsMatch(phone_input.Text, "^[0-9]{4}$"))
        {
            phone_ok = true;
            phone_input.TextRenderer.color = new Color32(50, 50, 50, 255);
        }
        else
        {
            phone_input.TextRenderer.color = Color.red;
            phone_ok = false;
        }
    }

    //public void VerifyIdFormat() {
    //    if (Regex.IsMatch(id_input.Text, @"^\d{13}$")) {
    //        //id_input.textComponent.color = new Color32(50, 50, 50, 255);
    //        id_input.TextRenderer.color = new Color32(50, 50, 50, 255);
    //        id_ok = true;
    //    } else {
    //        id_input.TextRenderer.color = Color.red;
    //        //id_ok = false;
    //    }
    //}

    public void UI_STARTOVER_BUTTON_PRESSED()
    {
        veredict = true;
        //App_Manager.instance.HideKeyboard();
        App_Manager.instance.prize_won = false;
        App_Manager.instance.error_msg = "";
        scrchc.ResetScratchCard();
        prog.Progreso = 0f;
        prog.ProgressFix();
        ShowMainPanel();
        sc.SetActive(false);
    }

    public void UI_LOGIN_BUTTON_PRESSED()
    {
        if (networking)
            return;

        if (user_input.Text.Equals("") || GetPassString().Equals(""))
        {
            AuremUtils.Log("Empty");
            App_Manager.instance.error_msg = "No deje los campos vacíos";
            ErrorLoginUIShow();
            return;
        }
        if (user_input.Text.Contains("SELECT") || user_input.Text.Contains("UPDATE") || user_input.Text.Contains("DELETE") || user_input.Text.Contains("DROP") || user_input.Text.Contains("INSERT"))
        {
            user_input.Text = "";
            return;
        }

        if (GetPassString().Contains("SELECT") || GetPassString().Contains("UPDATE") || GetPassString().Contains("DELETE") || GetPassString().Contains("DROP") || GetPassString().Contains("INSERT"))
        {
            SetPassString("");
            return;
        }

        AuremUtils.Log("Loging in");
        network_panel.SetActive(true);
        NetworkConnections.instance.OnLoginReturnResponse += A101Response;
        NetworkConnections.instance.OnLoginReturnError += ServerErrorResponses;
        App_Manager.instance.A101(user_input.Text, GetPassString());

    }

    public void UI_LOGOUT_BUTTON_PRESSED()
    {

        //App_Manager.instance.HideKeyboard();
        AuremUtils.Log("Logging out");
        network_panel.SetActive(true);
        NetworkConnections.instance.OnLogoutReturnResponse += A103Response;
        NetworkConnections.instance.OnLogoutReturnError += ServerErrorResponses;
        App_Manager.instance.A103();
        PlayerPrefs.DeleteAll();
    }

    public void UI_GENERATE_SCRATCH_PRESSED(string _code)
    {
        if (networking)
            return;
        network_panel.SetActive(true);
        NetworkConnections.instance.OnDrawPrizeReturnResponse += A110Response;
        NetworkConnections.instance.OnDrawPrizeReturnError += ServerErrorResponses;
        App_Manager.instance.A110(_code);

    }


    #endregion //UI_BUTTONS_PRESSED

    //public void CheckAuthCode() {
    //    NetworkConnections.OnReturnedResponse += A102Response;
    //    NetworkConnections.OnReturnedError += A102Response;
    //    App_Manager.instance.A102();
    //}

    #region HELPERS

    private string GetPassString()
    {
        if (password_hidden)
        {
            return pass_input.Text;
        }
        else
        {
            return pass_view_input.Text;
        }
    }

    private string SetPassString(string _s)
    {
        if (password_hidden)
        {
            return pass_input.Text = _s;
        }
        else
        {
            return pass_view_input.Text = _s;
        }
    }

    private void SetCategoryImage(string _prizename)
    {
        int cat_index = 12;
        //prize_lbl.text = _prizename;
        switch (_prizename)
        {

            case "Boletos al cine":
                cat_index = 0;
                break;
            case "Termo":
                cat_index = 1;
                break;
            case "Mini Parlante":
                cat_index = 2;
                break;
            case "Sombrilla":
                cat_index = 3;
                break;
            case "Gracias por participar":
                cat_index = 4;
                break;
            default:
                break;
        }
        prize_img.sprite = categories[cat_index];
    }
    #endregion

    #region SERVER_RESPONSES
    public void ServerErrorResponses(ErrorResponseData erd)
    {
        UI_Manager.instance.networking = false;
        network_panel.SetActive(false);
        switch (erd.errorCode)
        {
            case "101":
                NetworkConnections.instance.OnLoginReturnResponse -= A101Response;
                NetworkConnections.instance.OnLoginReturnError -= ServerErrorResponses;
                App_Manager.instance.error_msg = erd.errorMessage;
                ErrorLoginUIShow();
                tus_credenciales = 1;
                App_Manager.instance.ReleaseUserAuth();
                break;
            case "102":
                NetworkConnections.instance.OnDrawPrizeReturnResponse -= A110Response;
                NetworkConnections.instance.OnDrawPrizeReturnError -= ServerErrorResponses;
                break;
            case "103":
                NetworkConnections.instance.OnClaimPrizeReturnResponse -= A111Response;
                NetworkConnections.instance.OnClaimPrizeReturnError -= ServerErrorResponses;
                break;
            case "104":
                NetworkConnections.instance.OnLogoutReturnResponse -= A103Response;
                NetworkConnections.instance.OnLogoutReturnError -= ServerErrorResponses;
                break;
        }
        error_holder.gameObject.SetActive(true);
        err_txt.text = erd.errorMessage;
    }

    public void A111Response(ClaimPrizeResponseData cprd)
    {
        UI_Manager.instance.networking = false;
        network_panel.SetActive(false);
        NetworkConnections.instance.OnClaimPrizeReturnResponse -= A111Response;
        NetworkConnections.instance.OnClaimPrizeReturnError -= ServerErrorResponses;
        if (cprd.status.Equals("200"))
        {
            ShowGoBackPanel();
            App_Manager.instance.authing = false;
        }
        //NetworkConnections.OnReturnedResponse -= A111Response;
        //NetworkConnections.OnReturnedError -= A111Response;
        //AuremUtils.Log("LOGIN REPONSE: " + NetworkConnections.instance.responseText);
        //if (String.IsNullOrEmpty(NetworkConnections.instance.errorText)) {
        //    JSONObject j = new JSONObject(NetworkConnections.instance.responseText);
        //    if (j.keys[0].Equals("response")) {
        //        JSONObject obj = (JSONObject)j.list[0];
        //        if (obj.type == JSONObject.Type.OBJECT) {
        //            JSONObject success = (JSONObject)obj.list[0];
        //            if (success.b) {//SI EXISTE
        //                ShowGoBackPanel();
        //                App_Manager.instance.authing = false;
        //            } else {//NO EXISTE
        //                error_holder.gameObject.SetActive(true);
        //                err_txt.text = "Error de solicitud";
        //            }
        //        }
        //    }
        //} else {

        //}
    }

    public void A110Response(PrizeDrawResponseData dprd)
    {
        UI_Manager.instance.networking = false;
        network_panel.SetActive(false);
        NetworkConnections.instance.OnDrawPrizeReturnResponse -= A110Response;
        NetworkConnections.instance.OnDrawPrizeReturnError -= ServerErrorResponses;
        App_Manager.instance.RenewSessionTimeLimit();
        if (dprd.success)
        {
            prizename = dprd.prizeName;
            PlayerPrefs.SetString("prizename", dprd.prizeName);
            App_Manager.instance.CrossCodeFromList();
            App_Manager.instance.prize_won = true;
            SetCategoryImage(dprd.prizeName);
            ShowScratchPanel();
            ShowScratchCard();
            veredict = false;
        }
        else
        {
            App_Manager.instance.prize_won = false;
            App_Manager.instance.CrossCodeFromList();
            SetCategoryImage(dprd.prizeName);
            ShowScratchPanel();
            ShowScratchCard();
        }
        //NetworkConnections.OnReturnedResponse -= A110Response;
        //NetworkConnections.OnReturnedError -= A110Response;
        //AuremUtils.Log("LOGIN REPONSE: " + NetworkConnections.instance.responseText);
        //if (String.IsNullOrEmpty(NetworkConnections.instance.errorText)) {
        //    JSONObject j = new JSONObject(NetworkConnections.instance.responseText);
        //    if (j.keys[0].Equals("response")) {
        //        App_Manager.instance.RenewSessionTimeLimit();
        //        JSONObject obj = (JSONObject)j.list[0];
        //        if (obj.type == JSONObject.Type.OBJECT) {
        //            JSONObject success = (JSONObject)obj.list[0];
        //            if (success.b) { //USUARIO GANÓ
        //                App_Manager.instance.CrossCodeFromList();
        //                App_Manager.instance.prize_won = true;
        //                JSONObject prize = (JSONObject)obj.list[1];
        //                prizename = prize.str;
        //                PlayerPrefs.SetString("prizename", prizename);
        //                SetCategoryImage(prize.str);
        //                ShowScratchPanel();
        //                ShowScratchCard();
        //                veredict = false;
        //            } else { //USUARIO PERDIÓ
        //                JSONObject msg = (JSONObject)obj.list[1];
        //                string r = msg.str;
        //                veredict = false;
        //                switch (r) {
        //                    case "Código No es válido.":
        //                        error_holder.gameObject.SetActive(true);
        //                        err_txt.text = r;
        //                        break;
        //                    case "Su sesión no ha-sido autenticada -o há caducado.":
        //                        error_holder.gameObject.SetActive(true);
        //                        err_txt.text = r.Replace("-", "\n");
        //                        LogoutSuccess();
        //                        break;
        //                    case "Sesion no permitida.":
        //                        error_holder.gameObject.SetActive(true);
        //                        err_txt.text = r;
        //                        LogoutSuccess();
        //                        break;
        //                    case "Yá has usado-este código.":
        //                        error_holder.gameObject.SetActive(true);
        //                        err_txt.text = r.Replace("-", "\n"); ;
        //                        break;
        //                    case "Gracias por participar.":
        //                        App_Manager.instance.prize_won = false;
        //                        App_Manager.instance.CrossCodeFromList();
        //                        SetCategoryImage(msg.str);
        //                        ShowScratchPanel();
        //                        ShowScratchCard();
        //                        break;
        //                    case "Has sido bloqueado-por 5 intentos fallidos--Intenta más tarde.":
        //                        error_holder.gameObject.SetActive(true);
        //                        err_txt.text = r.Replace("-", "\n");
        //                        LogoutSuccess();
        //                        break;
        //                }

        //            }
        //        }

        //    }
        //} else {
        //    error_holder.gameObject.SetActive(true);
        //    err_txt.text = "Error procesando solicitud \nintenta más tarde.";
        //    LogoutSuccess();
        //}
    }

    public void ShowErrorDialog(string _msg)
    {
        error_holder.gameObject.SetActive(true);
        err_txt.text = _msg;
    }


    public void A103Response(LogoutResponseData lrd)
    {
        UI_Manager.instance.networking = false;
        network_panel.SetActive(false);
        NetworkConnections.instance.OnLogoutReturnResponse -= A103Response;
        NetworkConnections.instance.OnLogoutReturnError -= ServerErrorResponses;
        if (lrd.status.Equals("200"))
        {
            LogoutSuccess();
        }
        //NetworkConnections.OnReturnedResponse -= A103Response;
        //NetworkConnections.OnReturnedError -= A103Response;
        //AuremUtils.Log("LOGIN REPONSE: " + NetworkConnections.instance.responseText);
        //if (String.IsNullOrEmpty(NetworkConnections.instance.errorText)) {
        //    JSONObject j = new JSONObject(NetworkConnections.instance.responseText);
        //    if (j.keys[0].Equals("response")) {
        //        JSONObject obj = (JSONObject)j.list[0];
        //        if (obj.type == JSONObject.Type.OBJECT) {
        //            JSONObject success = (JSONObject)obj.list[0];
        //            if (success.b) {//SI EXISTE
        //                LogoutSuccess();
        //            } else {//NO EXISTE
        //            }
        //        }
        //    }
        //} else {

        //}
    }

    public void LogoutSuccess()
    {
        App_Manager.instance.ReleaseUserAuth();
        App_Manager.instance.data_set = false;
        if (App_Manager.instance.codes != null)
            App_Manager.instance.codes = null;
        ShowLoginPanel();
    }

    public void A101Response(LoginResponseData lrd)
    {
        UI_Manager.instance.networking = false;
        network_panel.SetActive(false);
        NetworkConnections.instance.OnLoginReturnResponse -= A101Response;
        NetworkConnections.instance.OnLoginReturnError -= ServerErrorResponses;
        // Debug.Log(JsonUtility.ToJson(lrd));
        if (lrd.status.Equals("200"))
        {
            App_Manager.instance.codes = new List<CodeButton>();
            PlayerPrefs.SetString("auth", lrd.auth);
            PlayerPrefs.SetString("prizename", lrd.lastWonPrize);
            App_Manager.instance.RenewSessionTimeLimit();
            App_Manager.instance.SetUserAuth(user_input.Text);
            foreach (CodesData c in lrd.codes)
            {
                App_Manager.instance.codes.Add(new CodeButton(c.transactionId, c.referenceId, c.enable));
            }

            if (lrd.notify)
            {
                notify_panel.SetActive(true);
            }
            user_input.Text = "";
            SetPassString("");
            ShowMainPanel();
        }



        //UI_Manager.instance.networking = false;
        //network_panel.SetActive(false);
        //NetworkConnections.OnReturnedResponse -= A101Response;
        //NetworkConnections.OnReturnedError -= A101Response;
        //AuremUtils.Log("LOGIN REPONSE: " + NetworkConnections.instance.responseText);
        //if (String.IsNullOrEmpty(NetworkConnections.instance.errorText)) {
        //    try {
        //        JSONObject j = new JSONObject(NetworkConnections.instance.responseText);
        //        if (j.keys[0].Equals("response")) {
        //            JSONObject obj = (JSONObject)j.list[0];
        //            if (obj.type == JSONObject.Type.OBJECT) {
        //                JSONObject success = (JSONObject)obj.list[0];
        //                if (success.b) {//SI EXISTE
        //                    JSONObject auth = (JSONObject)obj.list[1];
        //                    if (obj.list.Count > 2) {
        //                        Debug.Log("TOKEN KEY");
        //                        JSONObject tokenkey = (JSONObject)obj.list[2];
        //                        Debug.Log("XCSRF");
        //                        JSONObject xcsrf = (JSONObject)obj.list[3];
        //                        Debug.Log("D");
        //                        JSONObject d = (JSONObject)obj.list[4];
        //                        Debug.Log("COOKIE");
        //                        JSONObject cookie = (JSONObject)obj.list[5];
        //                        App_Manager.instance.RenewSessionTimeLimit();
        //                        App_Manager.instance.SetUserAuth(auth.str, user_input.Text, xcsrf.str, tokenkey.str, d.Print(false), cookie.Print(false));
        //                        Debug.Log("CODES LIST");
        //                        JSONObject codelist = (JSONObject)obj.list[6];
        //                        App_Manager.instance.codes = new List<CodeButton>();
        //                        for (int i = 0; i < codelist.list.Count; i++) {
        //                            JSONObject row = (JSONObject)codelist.list[i];
        //                            string c = row.list[1].str;
        //                            string r = row.list[0].str;
        //                            bool en = row.list[3].b;
        //                            App_Manager.instance.codes.Add(new CodeButton(c, r, en));
        //                        }
        //                        JSONObject notify = (JSONObject)obj.list[7];

        //                        if (notify.str != "none") {
        //                            notify_panel.SetActive(true);
        //                        }
        //                        tus_credenciales = 0;

        //                    } else {
        //                        App_Manager.instance.SetUserAuth(auth.str, user_input.Text, "", "", "", "");
        //                    }
        //                    AuremUtils.Log(auth.str);
        //                    user_input.Text = "";
        //                    SetPassString("");
        //                    ShowMainPanel();
        //                } else {//NO EXISTE
        //                        //USUARIO NO EXISTE
        //                    JSONObject msg = (JSONObject)obj.list[1];
        //                    string err = msg.str.Replace("-", "\n");

        //                    if (msg.str.Contains("Tus credenciales") && tus_credenciales == 1) {
        //                        string error = "Servidores ocupados-intenta más tarde";
        //                        err = error.Replace("-", "\n");
        //                    }
        //                    App_Manager.instance.error_msg = err;
        //                    ErrorLoginUIShow();
        //                    App_Manager.instance.ReleaseUserAuth();
        //                }
        //            }
        //        }

        //    } catch (Exception e) {

        //        using (StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/error_logs.txt")) {
        //            writer.WriteLine(App_Manager.instance.db_call.text);
        //        }
        //        string mssg = "Servidores ocupados-intente más tarde.";
        //        string err = mssg.Replace("-", "\n");
        //        App_Manager.instance.error_msg = err;
        //        ErrorLoginUIShow();
        //        tus_credenciales = 1;

        //    }
        //}

    }

    #endregion //SERVER_RESPONSES
}
