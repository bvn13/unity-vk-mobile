using UnityEngine;
using System.Collections;

using Text;

public class VKApi : MonoBehaviour
{
	public string ClientID = "";
	public string UserID = "";
	public string Scope = "friends,photos,wall";
	public string AccessToken = "";
	private int ExpiresIn = 0;
	public const string OAUTH_URL = "https://oauth.vk.com/authorize?client_id=CLIENT_ID&scope=SCOPE&redirect_uri=https://oauth.vk.com/blank.html&response_type=token&display=mobile&v=VERSION";
	public const string API_URL = "https://api.vk.com/method/METHOD_NAME?PARAMETERS&access_token=ACCESS_TOKEN&owner_id=USER_ID&v=VERSION";
//	public static string _accessToken;
	private static WebViewObject _webViewObject;
	public string Version = "5.27";
	
	public static event OAuthEventHandler OAuthEvent;
	public static event ApiSuccessEventHandler ApiSuccessEvent;
	public static event ApiErrorEventHandler ApiErrorEvent;

	public delegate void OAuthEventHandler (bool success, string error = "", string error_reason = "");
	public delegate void ApiSuccessEventHandler (string methodName,string data);
	public delegate void ApiErrorEventHandler (string methodName,string data);


	private static VKApi _vk;
	
	public bool isAuthed = false;
	

	private enum Stage {
		AuthRequestSent,
		AuthRequestReceived,
		AuthCompleted
	}

	private Stage _stage;
	
	void Start ()
	{
			
	}
	
	void Awake ()
	{
		DontDestroyOnLoad (transform.gameObject);
		
		_vk = (VKApi)GetComponent<VKApi> ();
	}
	
	public static void InitWebview ()
	{	
		bool isMobile = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
		//Debug.LogWarning ("isMobile: " + isMobile);
		
		if (isMobile) {
			
			_webViewObject =
			(new GameObject ("WebViewObject")).AddComponent<WebViewObject> ();

			_webViewObject.OnRedirectingToEvent += HandleOnRedirectingToEvent;
			_webViewObject.OnLoadUrlEvent += HandleOnLoadUrlEvent;

			_webViewObject.Init (
				/*
				(token) => {
					//Debug.LogWarning ("AccessToken " + token);
					CloseWebView();
					_vk.AccessToken = token;
					if (OAuthEvent != null) {
						OAuthEvent (true);
					}				
				}
				*/
			);
			
			var oauth_url = OAUTH_URL.Replace ("CLIENT_ID", _vk.ClientID);
			oauth_url = oauth_url.Replace ("SCOPE", _vk.Scope);	
			oauth_url = oauth_url.Replace ("VERSION", _vk.Version);
			
			//Debug.LogWarning ("oauth_url: " + oauth_url);

			_vk._stage = Stage.AuthRequestSent;
			_webViewObject.LoadURL (oauth_url);
			_webViewObject.SetVisibility (true);
		
		} else {
			Debug.LogWarning("AccessToken " + _vk.AccessToken);
			if (OAuthEvent != null) {
				OAuthEvent (true);
			}
		}
	}


	static void HandleOnLoadUrlEvent (string data)
	{
		Debug.LogWarning("VK.LoadedURL: "+data);
		string[] urlNcontent = data.Split(new string[]{ "#|#|#" }, System.StringSplitOptions.None);
		string url = "";
		string content = "";
		Debug.LogWarning("VK.LoadedURL.ArrayLength: "+urlNcontent.Length);
		if (urlNcontent.Length == 2) {
			url = urlNcontent[0];
			content = urlNcontent[1];
		}
		Debug.LogWarning("VK.LoadedURL.URL: "+url);
		Debug.LogWarning("VK.LoadedURL.Content: "+content);
		if (_vk._stage == Stage.AuthRequestSent) {
			bool success = true;
			var resp = JSON.JsonDecode(data, ref success) as Hashtable;
			if (!success || resp.ContainsKey("error")) {
				if (ApiErrorEvent != null) {
					ApiErrorEvent(_vk.currentMethodName, data);
				}
			} else {
				if (ApiSuccessEvent != null) {
					ApiSuccessEvent(_vk.currentMethodName, data);
				}
			}
		}
	}

	static void HandleOnRedirectingToEvent (string url)
	{
		Debug.LogWarning("VK.RedirectedTO: "+url+" - PARSE IT");
		if (url.StartsWith("https://oauth.vk.com/blank.html#")) {
			Debug.LogWarning("TOKEN");
			string urlpart = url.Replace("https://oauth.vk.com/blank.html#", "");
			string[] pars = urlpart.Split("&".ToCharArray());
			Debug.LogWarning("PARS: "+pars.ToString());
			string error = "";
			string error_reason = "";
			foreach (string par in pars) {
				Debug.LogWarning("PAR: "+par.ToString());
				string[] keyval = par.Split("=".ToCharArray());
				if (keyval.Length != 2) {
					continue;
				} else {
					if (keyval[0].Equals("access_token")) {
						Debug.LogWarning("GOT ACCESS TOKEN!!!");
						_vk.AccessToken = keyval[1];
						_vk._stage = Stage.AuthCompleted;
					} else if (keyval[0].Equals("user_id")) {
						Debug.LogWarning("USER ID: "+keyval[1]);
						_vk.UserID = keyval[1];
					} else if (keyval[0].Equals("expires_in")) {
						_vk.ExpiresIn = int.Parse(keyval[1]);
					} else if (keyval[0].Equals("error")) {
						error = keyval[1];
					} else if (keyval[0].Equals("error_reason")) {
						error_reason = keyval[1];
					}
				}
			}
			_vk.isAuthed = !string.IsNullOrEmpty(_vk.AccessToken) && !string.IsNullOrEmpty(_vk.UserID) && _vk.ExpiresIn > 0;
			if (OAuthEvent != null) {
				OAuthEvent(_vk.isAuthed, error, error_reason);
			}
			CloseWebView();
		} else {
			Debug.LogWarning("NOT TOKEN");
		}
	}

	public static void CloseWebView() {
		_webViewObject.SetVisibility(false);
		_webViewObject.OnRedirectingToEvent -= HandleOnRedirectingToEvent;
		_webViewObject.OnLoadUrlEvent -= HandleOnLoadUrlEvent;
		Destroy (_webViewObject);
	}

	public static void Init ()
	{
		InitWebview ();
	}
	
	public static void Init (string clientID, string scope)
	{
		_vk.ClientID = clientID;
		_vk.Scope = scope;

		InitWebview ();
	}

	private string currentMethodName = "";

	public static void Api (string methodName, Hashtable parameters)
	{
		_vk.CallApi (methodName, parameters);
	}
	
	public void CallApi (string methodName, Hashtable parameters)
	{
		currentMethodName = methodName;
		StartCoroutine (_CallApi (methodName, parameters));
	}
	
	private IEnumerator _CallApi (string methodName, Hashtable parameters)
	{
		if (!isAuthed) {
			if (ApiErrorEvent != null) {
				ApiErrorEvent(methodName, "NOT_AUTHED");
				yield break;
			}
		}

		string url = API_URL.Replace ("METHOD_NAME", methodName);
							
		string paramValues = "";

		bool isFirst = true;
		foreach (var item in parameters.Keys) {
			if (!isFirst)
				paramValues += "&";
			paramValues += item + "=" + WWW.EscapeURL (parameters [item].ToString ());
			isFirst = false;
		}
		
		url = url.Replace ("PARAMETERS", paramValues);
		url = url.Replace ("USER_ID", UserID);
		url = url.Replace ("ACCESS_TOKEN", _vk.AccessToken);
		url = url.Replace ("VERSION", _vk.Version);
		
		Debug.LogWarning ("url: " + url);
		
		WWW www = new WWW (url);
		yield return www;
		Debug.LogWarning ("www.text: " + www.text);

		bool success = true;
		var resp = JSON.JsonDecode(www.text, ref success) as Hashtable;
		if (!success || resp.ContainsKey("error")) {
			if (ApiErrorEvent != null) {
				ApiErrorEvent(currentMethodName, www.text);
			}
		} else {
			if (ApiSuccessEvent != null) {
				ApiSuccessEvent(currentMethodName, www.text);
			}
		}
	}
	

	
	
	
	
	
	
}
