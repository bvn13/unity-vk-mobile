using UnityEngine;
using System.Collections;
using SimpleJson;
using System.Collections.Generic;
using System;

public delegate void OAuthEventHandler (bool success);

public delegate void ApiSuccessEventHandler (string methodName,string data);

public delegate void ApiErrorEventHandler (string methodName,string data);

public delegate void PostWallEventHandler (bool success);

public enum Users
{
	Get,
	Search,
	IsAppUser,
	GetSubscriptions,
	GetFollowers
}
	
public enum Groups
{
	IsMember,
	GetById ,
	Get,
	GetMembers,
	Join,
	Leave,
	Search,
	GetInvites,
	BanUser,
	UnbanUser,
	GetBanned
}

public enum Friends
{
	Get,
	GetOnline,
	GetMutual,
	GetRecent,
	GetRequests,
	Add,
	Edit,
	Delete,
	GetLists,
	AddList,
	EditList,
	DeleteList,
	GetAppUsers,
	GetByPhones,
	DeleteAllRequests,
	GetSuggestions,
	AreFriends		
}
	
public enum Wall
{
	Get,
	GetById,
	SavePost,
	Post,
	Repost,
	GetReposts,
	Edit,
	Delete,
	Restore,
	GetComments,
	AddComment,
	DeleteComment, 
	RestoreComment,
	GetLikes, 
	AddLike, 
	DeleteLike
}

public enum Photos
{
	GetWallUploadServer,
	SaveWallPhoto
}

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
	public static event PostWallEventHandler PostWallEvent;
	
	private static VK _vk;
	private static Hashtable _methods = new Hashtable ();
	
	void InitMethods ()
	{
		// Users (ПОЛЬЗОВАТЕЛИ) 
		_methods.Add (Users.Get, "users.get");
		_methods.Add (Users.Search, "users.search");
		_methods.Add (Users.IsAppUser, "users.isAppUser");
		_methods.Add (Users.GetSubscriptions, "users.getSubscriptions");
		_methods.Add (Users.GetFollowers, "users.getFollowers");
		
		// Groups (Группы) 
		_methods.Add (Groups.IsMember, "groups.isMember");
		_methods.Add (Groups.GetById, "groups.getById");
		_methods.Add (Groups.Get, "groups.get");
		_methods.Add (Groups.GetMembers, "groups.getMembers");
		_methods.Add (Groups.Join, "groups.join");
		_methods.Add (Groups.Leave, "groups.leave");
		_methods.Add (Groups.Search, "groups.search");
		_methods.Add (Groups.GetInvites, "groups.getInvites");
		_methods.Add (Groups.BanUser, "groups.banUser");
		_methods.Add (Groups.UnbanUser, "groups.unbanUser");
		_methods.Add (Groups.GetBanned, "groups.getBanned");
		
		// Friends (Друзья) 
		_methods.Add (Friends.Get, "friends.get");
		_methods.Add (Friends.GetOnline, "friends.getOnline");
		_methods.Add (Friends.GetMutual, "friends.getMutual");
		_methods.Add (Friends.GetRecent, "friends.getRecent");
		_methods.Add (Friends.GetRequests, "friends.getRequests");
		_methods.Add (Friends.Add, "friends.add");
		_methods.Add (Friends.Edit, "friends.edit");
		_methods.Add (Friends.Delete, "friends.delete");
		_methods.Add (Friends.GetLists, "friends.getLists");
		_methods.Add (Friends.AddList, "friends.addList");
		_methods.Add (Friends.EditList, "friends.editList");
		_methods.Add (Friends.DeleteList, "friends.deleteList");
		_methods.Add (Friends.GetAppUsers, "friends.getAppUsers");
		_methods.Add (Friends.GetByPhones, "friends.getByPhones");
		_methods.Add (Friends.DeleteAllRequests, "friends.deleteAllRequests");
		_methods.Add (Friends.GetSuggestions, "friends.getSuggestions");
		_methods.Add (Friends.AreFriends, "friends.areFriends");
		
		// Wall (Стена) 
		_methods.Add (Wall.Get, "wall.get");
		_methods.Add (Wall.GetById, "wall.getById");
		_methods.Add (Wall.SavePost, "wall.savePost");
		_methods.Add (Wall.Post, "wall.post");
		_methods.Add (Wall.Repost, "wall.repost");
		_methods.Add (Wall.GetReposts, "wall.getReposts");
		_methods.Add (Wall.Edit, "wall.edit");
		_methods.Add (Wall.Delete, "wall.delete");
		_methods.Add (Wall.Restore, "wall.restore");
		_methods.Add (Wall.GetComments, "wall.getComments");
		_methods.Add (Wall.AddComment, "wall.addComment");
		_methods.Add (Wall.DeleteComment, "wall.deleteComment");
		_methods.Add (Wall.RestoreComment, "wall.restoreComment");
		_methods.Add (Wall.GetLikes, "wall.getLikes");
		_methods.Add (Wall.AddLike, "wall.addLike");
		_methods.Add (Wall.DeleteLike, "wall.deleteLike");

		// Photos ()
		_methods.Add (Photos.GetWallUploadServer, "photos.getWallUploadServer");
		_methods.Add (Photos.SaveWallPhoto, "photos.saveWallPhoto");
		
	}
	
	void Start ()
	{
		InitMethods ();
				
//		string message = "{\"response\":{\"post_id\":2577}}";
//		string message = "{\"error\":{\"error_code\":5,\"error_msg\":\"User authorization failed: invalid access_token.\",\"request_params\":[{\"key\":\"oauth\",\"value\":\"1\"},{\"key\":\"method\",\"value\":\"wall.post\"},{\"key\":\"message\",\"value\":\"Hello world!\"},{\"key\":\"owner_id\",\"value\":\"1554500\"},{\"key\":\"access_token\",\"value\":\"ACCESS_TOKEN\"}]}}";
//		var json = (IDictionary<string, object>)SimpleJson.SimpleJson.DeserializeObject (message);
//		Debug.Log ("response: " + json.ContainsKey ("response"));
		
//		JsonObject response = (JsonObject)json ["response"];
		
		
		
	}
	
	public WWW GetWWW (string methodName, Hashtable parameters)
	{
		string url = API_URL.Replace ("METHOD_NAME", methodName);							
		string paramValues = "";
		
		foreach (var item in parameters.Keys) {
			paramValues += item + "=" + WWW.EscapeURL (parameters [item].ToString ()) + "&";
		}		
		url = url.Replace ("PARAMETERS", paramValues);
		url = url.Replace ("ACCESS_TOKEN", _vk.AccessToken);
		
		Debug.Log ("url: " + url);
		
		return new WWW (url);
	}
	
	public WWW GetWWW (object methodName, Hashtable parameters)
	{
		var method = _methods [methodName].ToString ();
		return GetWWW (method, parameters);
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
		}
	}
	
	/// <summary>
	/// Init this instance.
	/// </summary>
	public static void Init ()
	{
		InitWebview ();
	}
	
	/// <summary>
	/// Init the specified clientID and scope.
	/// </summary>
	/// <param name='clientID'>
	/// Client I.
	/// </param>
	/// <param name='scope'>
	/// Scope.
	/// </param>
	public static void Init (string clientID, string scope)
	{
		_vk.ClientID = clientID;
		_vk.Scope = scope;
		
		InitWebview ();
	}
	
	public static void Api (string methodName, Hashtable parameters)
	{
		_vk.StartCoroutine (_vk._CallApi (methodName, parameters));
	}
	
	public static void Api (object methodName, Hashtable parameters)
	{
		string method = _methods [methodName].ToString ();
		_vk.StartCoroutine (_vk._CallApi (method, parameters));
	}
	
	private IEnumerator _CallApi (string methodName, Hashtable parameters)
	{		
		WWW www = GetWWW (methodName, parameters);
		yield return www;		
		
		var json = (IDictionary<string, object>)SimpleJson.SimpleJson.DeserializeObject (www.text);
		bool isSuccess = json.ContainsKey ("response");
		if (isSuccess) {
			Debug.Log ("www.text: " + www.text);	
			if (ApiSuccessEvent != null) {
				ApiSuccessEvent (methodName, www.text);
			}
		} else {
			Debug.LogError ("www.text: " + www.text);
			if (ApiErrorEvent != null) {
				ApiErrorEvent (methodName, www.text);
			}
		}		
	}
	
	public static string WallUploadServer = "";
	private bool _isPostWall = false;
	private int _wallUID;
	private string _wallDescription;
	private Texture2D photo1;
	
	public static void PostWall (int uid, Texture2D photo, string description)
	{
		_vk._PostWall (uid, photo, description);
	}
	
	public void _PostWall (int uid, Texture2D photo, string description)
	{
		
		_wallUID = uid;		
		_isPostWall = true;
		photo1 = photo;		
		_wallDescription = String.IsNullOrEmpty (description) ? string.Empty : description;
		
//		Debug.Log("_wallDescription: "+_wallDescription);
		
		if (String.IsNullOrEmpty (WallUploadServer)) {
			StartCoroutine (_GetWallUploadServer ());
		} else {
			StartCoroutine (_UploadWallImage ());
		}
	}
	
	private IEnumerator _GetWallUploadServer ()
	{
//		var url = "https://api.vk.com/method/photos.getWallUploadServer?access_token=" + _vk.AccessToken;
		
		
//		WWW www = GetWWW()
			
		WWW www = GetWWW (Photos.GetWallUploadServer, new Hashtable ());
		yield return www;
		
		var json = (IDictionary<string, object>)SimpleJson.SimpleJson.DeserializeObject (www.text);
		if (json.ContainsKey ("response")) {
			JsonObject response = (JsonObject)json ["response"];
		
			WallUploadServer = (string)response ["upload_url"];
			Debug.Log ("WallUploadServer: " + WallUploadServer);
			
			if (_isPostWall) {
				StartCoroutine (_UploadWallImage ());
			}
		}
	}
	
	private IEnumerator _UploadWallImage ()
	{
		yield return new WaitForEndOfFrame();

		var bytes = photo1.EncodeToPNG ();
		
		var form = new WWWForm ();
		form.AddBinaryData ("photo", bytes, "photo.png", "image/png");
		// Upload to a cgi script
		var w = new WWW (WallUploadServer, form);
		yield return w;
		if (!String.IsNullOrEmpty (w.error))
			print (w.error);
		else
			print ("Finished Uploading Screenshot");
		
		Debug.Log ("www.text: " + w.text);
		
		var json = (IDictionary<string, object>)SimpleJson.SimpleJson.DeserializeObject (w.text);
		
		Hashtable data = new Hashtable ();
		data ["server"] = json ["server"] .ToString ();
		data ["hash"] = json ["hash"] .ToString ();
		data ["photo"] = json ["photo"] .ToString ();
		StartCoroutine (_SaveWallPhoto (data));		
	}
	
	private IEnumerator _SaveWallPhoto (Hashtable data)
	{
		string url = "https://api.vk.com/method/photos.saveWallPhoto";
		
		Hashtable form = new Hashtable ();
		form.Add ("uid", _wallUID);
		form.Add ("server", data ["server"].ToString ());
		form.Add ("photo", data ["photo"].ToString ());
		form.Add ("hash", data ["hash"].ToString ());
		
		WWW www = GetWWW (Photos.SaveWallPhoto, form);
		yield return www;
		Debug.Log ("www.text: " + www.text);	
		
		var json = (IDictionary<string, object>)SimpleJson.SimpleJson.DeserializeObject (www.text);
		
		JsonArray response = (JsonArray)json ["response"];
		JsonObject jdata = (JsonObject)response [0];
		
		var id = jdata ["id"] .ToString ();
		Debug.Log ("id: " + id);
		StartCoroutine (_DoWallPost (id));
	}
	
	private IEnumerator _DoWallPost (string attachments)
	{
		Hashtable data = new Hashtable ();
		data.Add ("attachments", attachments);
		data.Add ("owner_id", _wallUID);
		
		if (!String.IsNullOrEmpty (_wallDescription))
			data.Add ("message", _wallDescription);
		
		WWW www = GetWWW (Wall.Post, data);
		yield return www;
		Debug.Log ("www.text: " + www.text);
		
		if (PostWallEvent != null){
			var json = (IDictionary<string, object>)SimpleJson.SimpleJson.DeserializeObject (www.text);
			bool success = json.ContainsKey("response");
			PostWallEvent(success);
		}
	}
	
	
}