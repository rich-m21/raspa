using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System.IO;

public class controller : MonoBehaviour {
	public static controller instance;
	public bool debugEnabled = true;
	private WWW db_call;
	private Connections_Manager CM;
	private string url;
	private bool isAuthenticated;
	// Canvas
	public RectTransform[] panels; //register 0, generar 1, raspa 2, login 3
//	public RectTransform registrar;
//	public RectTransform generar;
//	public RectTransform raspa;
//	public RectTransform login;
	// Register Panel Inputs
	public InputField nombre;
	public InputField identidad;
	public InputField correo;
	public InputField telefono;
	public InputField codigo;
	//Login Panel Inputs
	public InputField email;
	public InputField pass;
	// checkboxes
	public Toggle tarjeta;
	public Toggle cuenta;
	public Toggle prestamo;
	// Labels
	public Text ProblemasCvs3;
	public Text ProblemasCvs2;
	public Text ProblemasCvs1;
	public Text LB_premio;
	public Text LB_premioGanador;
	//scratch
	public GameObject ScratchCard;
	// Funciones

	void Awake(){
		Connections_Manager.OnReturnedResponse += CheckConnectionsManager;
		Connections_Manager.OnReturnedError += CheckConnectionsManager;
	}
	void Start() {
		instance = this;
		CM = this.gameObject.GetComponent<Connections_Manager>();
		ScratchCard.SetActive(false);
		AppStart();

	}
	//PLAYERPREFS
	// Auth IF APP IS AUTHENTICATED
	private void AppStart(){
		if (PlayerPrefs.GetString("AUTH", "N").Equals("N")) {
			ShowLoginPanel();
		}
		else {
			ShowGenerarPanel();
		}
	}

	public void ShowRegisterPanel(){
		ShowPanelNumber(0);
	}

	public void ShowGenerarPanel(){
		ShowPanelNumber(1);
	}

	public void ShowRaspaPanel(){
		ShowPanelNumber(2);
	}

	public void ShowLoginPanel(){
		ShowPanelNumber(3);
	}

	private void ShowPanelNumber(int _n){
		for (int i = 0; i < panels.Length; i++) {
			if (i == _n) {
				panels[i].gameObject.SetActive(true);
				Debug.Log(_n);
			}
			else {
				panels[i].gameObject.SetActive(false);
			}
			if (_n == 2) {
				ScratchCard.SetActive(true);
			}
			else {
				ScratchCard.SetActive(false);
			}
		}
	}

//	private void showPanel(int canvas) {
//		
//		registrar.gameObject.SetActive(false);
//		generar.gameObject.SetActive(false);
//		raspa.gameObject.SetActive(false);
//		this.ScratchCard.SetActive(false);
//		switch (canvas) {
//			case 1:
//				registrar.gameObject.SetActive(true);
//				break;
//			case 2:
//				generar.gameObject.SetActive(true);
//				break;
//			case 3:
//				raspa.gameObject.SetActive(true);
//				this.ScratchCard.SetActive(true);
//				break;
//		}
//	}

	public void Registrar() {
		ProblemasCvs1.color = Color.red;
		string problemas = "";
		if (UI_Checker_Email(correo)) {
			if (validar(this.nombre) && validar(this.identidad) && validar(this.correo) && validar(this.telefono)) {
				if (tarjeta.isOn || prestamo.isOn || cuenta.isOn) {
					string nombre = this.nombre.text;
					Debug.Log("Nombre: " + nombre);
					string identidad = this.identidad.text;
					Debug.Log("identidad: " + identidad);
					string correo = this.correo.text;
					Debug.Log("correo: " + correo);
					string telefono = this.telefono.text;
					Debug.Log("teléfono: " + telefono);
					//guardar en la base de datos

					//mostrar canvas 2
					PlayerPrefs.SetString("AUTH","Y");
					ShowGenerarPanel();
				}
				else {
					problemas = "Necesita seleccionar un producto";
				}
			}
			else {
				problemas = "No puede dejar campos en blanco";
			}
		}
		else {
			problemas = "Formato de correo no válido";
		}
		ProblemasCvs1.text = problemas;
	}


	public void Ingresar(){
		ProblemasCvs3.color = Color.red;
		string problemas = "";
		if (UI_Checker_Email(email)) {
			if (validar(email) && validar(pass)) {
				
			}
			else {
				problemas = "No puede dejar campos en blanco";
			}
		}
		else {
			problemas = "Formato de Correo no Válido";
		}
		ProblemasCvs3.text = problemas;
	}
	public void revisar() {
		if (validar(this.codigo)) {
			consultarPool(this.codigo.text);
			ShowRaspaPanel();
		}
		else {
			ProblemasCvs2.color = Color.red;
			ProblemasCvs2.text = "Ingrese un código";
		}
	}

	void consultarPool(string codigo) {
		//consultar al servicio el premio.
		LB_premio.text = "Ipad Mini";
	}

	bool validar(InputField campo, bool codigo = false) {
		//metodo de validación aun no definido.
		if (campo.text.Length > 0) {
			return true;
		}
		return false;
	}
		

	public void reclamarPremio() {
		
	}

	public bool UI_Checker_Email(InputField _field) {
		if (Regex.IsMatch(_field.text, "^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-\\w]*[0-9a-z]*\\.)+[a-z0-9][\\-a-z0-9]{0,22}[a-z0-9]))$")) {
			return true;
		}
		return false;
	}

	#region NETWORK_FUNCTIONS
	public void RegisterDevice() {//R|[Y/N]|AUTH_CODE|DB_VER
		//Register in server
		url = "https://cloud-aurem.com/platform/service.php?";
		url += "action=A100&";
		url += "device_id=" + WWW.EscapeURL(SystemInfo.deviceUniqueIdentifier, System.Text.Encoding.UTF8) + "&";
		url += "device_model=" + WWW.EscapeURL(SystemInfo.deviceModel, System.Text.Encoding.UTF8) + "&";
		url += "device_os=" + WWW.EscapeURL(SystemInfo.operatingSystem, System.Text.Encoding.UTF8) + "&";
		url += "app_id=" + WWW.EscapeURL(Application.identifier, System.Text.Encoding.UTF8);
		//Debug.Log("REGISTER DEVICE: " + url);
		db_call = CM.GET(url);
	}

	private void CheckConnectionsManager() {
		if (db_call.error == null) {
			string response = AuremUtils.RemoveControlCharacters(db_call.text);
			if (response.StartsWith("P")) { // Save Profile
				if (controller.instance.debugEnabled)
					Debug.Log("Profile return String: " + db_call.text);
				string[] resp = db_call.text.Split('|');
				if (resp[1].Equals("N")) {
					//SAVE Profile info
					//					PlayerPrefs.SetString("AUTH_TOKEN", resp[2]);
					//					PlayerPrefs.SetString("NAME", resp[3]);
					//					PlayerPrefs.SetString("EMAIL", resp[2]);
					//					PlayerPrefs.SetString("PHONE", resp[2]);
					//					PlayerPrefs.SetString("DOB", resp[2]);
					//					PlayerPrefs.SetString("GNDR", resp[2]);
					//					PlayerPrefs.SetString("SHARE", "Y");
				}
				else {
					//ShowProfileMsg("Prefil Guardado", true);
//					SetMsgDialogText("Perfil Guardado");
//					Invoke("HideMsgDialog", 5f);
				}
			}
			if (response.StartsWith("L")) {
				string[] resp = response.Split('|');
				if (resp[1].Equals("N")) {
					//SAVE Profile info

				}
				else if (resp[1].Equals("Y")) {
					isAuthenticated = true;
					ShowGenerarPanel();
				}
			}
			else if (response.StartsWith("R")) { //Register Device
				if (controller.instance.debugEnabled)
					Debug.Log("Register return String: " + db_call.text);
				string[] resp = db_call.text.Split('|');
				if (resp[1].Equals("Y") || resp[1].Equals("U")) {
					if (controller.instance.debugEnabled)
						Debug.Log("SETTING PREFES AT REGISTER");
					PlayerPrefs.SetString("FRSTIME", "N");
					PlayerPrefs.SetString("AUTH_TOKEN", resp[2]);
					PlayerPrefs.SetString("DB_VER", resp[3]);

				}
				else {
					PlayerPrefs.SetString("AUTH_TOKEN", "0");
				}
			}



		}
		else {
			if (url.Contains("action=A105")) {
				using (StreamWriter sw = File.AppendText(Application.persistentDataPath + "/reg_log.txt")) {
					sw.WriteLine(url);
				}
			}
		}
	}


	#endregion //NETWORK_FUNCTIONS


}
