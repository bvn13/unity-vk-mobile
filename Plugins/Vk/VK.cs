using UnityEngine;
using System.Collections;

public delegate void OAuthEventHandler (bool success);

public delegate void ApiSuccessEventHandler (string methodName,string data);

public delegate void ApiErrorEventHandler (string methodName,string data);

public class VK:MonoBehaviour
{
	public string ClientID = "";
	public string Scope = "friends,photos,wall";
	public string AccessToken = "";
	public const string OAUTH_URL = "https://oauth.vk.com/authorize?client_id=CLIENT_ID&scope=SCOPE&redirect_uri=https://oauth.vk.com/blank.html&response_type=token&display=touch";
	public const string API_URL = "https://api.vk.com/method/METHOD_NAME?PARAMETERS&access_token=ACCESS_TOKEN";
//	public static string _accessToken;
	private static WebViewObject _webViewObject;
	
	public static event OAuthEventHandler OAuthEvent;
	public static event ApiSuccessEventHandler ApiSuccessEvent;
	public static event ApiErrorEventHandler ApiErrorEvent;
	
	private static VK _vk;
	
	
	
	public enum Users
	{
		Get = 1,
		Search = 2,
		IsAppUser = 3,
		GetSubscriptions = 4,
		GetFollowers = 5
	}
	
	public enum Groups
	{
		isMember = 11,
		getById = 12,
		Get = 13,
		getMembers = 14,
		join = 15,
		leave = 16,
		Search = 17,
		getInvites = 18,
		banUser = 19,
		unbanUser = 20,
		getBanned = 21
	}
	
	
	
	void Start ()
	{
			
	}
	
	void Awake ()
	{
		DontDestroyOnLoad (transform.gameObject);
		
		_vk = (VK)GetComponent<VK> ();
	}
	
	public static void InitWebview ()
	{	
		bool isMobile = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
		Debug.Log ("isMobile: " + isMobile);
		
		if (isMobile) {
			
			_webViewObject =
			(new GameObject ("WebViewObject")).AddComponent<WebViewObject> ();
		
			_webViewObject.Init ((token) => {
				Debug.Log ("AccessToken " + token);
//				_webViewObject.SetVisibility (false);
				Destroy (_webViewObject);
				_vk.AccessToken = token;
				if (OAuthEvent != null) {
					OAuthEvent (true);
				}				
			});
			
			var oauth_url = OAUTH_URL.Replace ("CLIENT_ID", _vk.ClientID);
			oauth_url = oauth_url.Replace ("SCOPE", _vk.Scope);	
			
			Debug.Log ("oauth_url: " + oauth_url);
						
			
			
			_webViewObject.LoadURL (oauth_url);
			_webViewObject.SetVisibility (true);
		} else {
			Debug.Log ("AccessToken " + _vk.AccessToken);
			if (OAuthEvent != null) {
				OAuthEvent (true);
			}
			//StartCoroutine ("GetWallUploadServer");
		}
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
	
	public static void Api (string methodName, Hashtable parameters)
	{
		_vk.CallApi (methodName, parameters);
	}
	
	public void CallApi (string methodName, Hashtable parameters)
	{
		StartCoroutine (_CallApi (methodName, parameters));
	}
	
	private IEnumerator _CallApi (string methodName, Hashtable parameters)
	{
		string url = API_URL.Replace ("METHOD_NAME", methodName);
							
		string paramValues = "";
		
		foreach (var item in parameters.Keys) {
			paramValues += item + "=" + WWW.EscapeURL (parameters [item].ToString ()) + "&";
		}
		
		url = url.Replace ("PARAMETERS", paramValues);
		url = url.Replace ("ACCESS_TOKEN", _vk.AccessToken);
		
		Debug.Log ("url: " + url);
		
		WWW www = new WWW (url);
		yield return www;
		Debug.Log ("www.text: " + www.text);	
	}
	
	
	
	
	
	
	
	
}
