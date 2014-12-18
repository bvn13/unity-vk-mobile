using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class VK {

	public class Friend {
		public enum SEX {
			female,// = 1,
			male,// = 2,
			unknown// = 0
		};
		public string id;
		public string first_name;
		public string last_name;
		public string deactivated;
		public bool isDeactivated {
			get { return deactivated.Equals("deleted") || deactivated.Equals("banned"); }
		}
		public string photo_id;
		public string sex;
		public SEX Sex {
			get { 
				if (sex.Equals("1"))
					return SEX.female;
				else if (sex.Equals("2"))
					return SEX.male;
				else return SEX.unknown;
			}
		}
		public string photo_100;
		public string photo_200;
		public string online;
		public bool isOnline {
			get { return online.Equals("1"); }
		}
		public string online_mobile;
		public bool isOnlineMobile {
			get { return online_mobile.Equals("1"); }
		}
	}

	private readonly string _reqSendToWall = "wall.post";
	private readonly string _reqFriendsGet = "friends.get";

	private readonly string _reqGetFriends_ShortList_Fields = ""; // empty please
	private readonly string _reqGetFriends_FullList_Fields = "";

	public delegate void AuthEventHandler (bool success, string error = "", string error_reason = "");
	public delegate void AuthEventHandlerShort (bool success);
	public delegate void SuccessEventHandler (string methodName, string data);
	public delegate void ErrorEventHandler (string methodName, string data);

	private AuthEventHandler cbAuthEvent;
	private AuthEventHandlerShort cbAuthEventShort;
	private SuccessEventHandler cbSuccessEvent;
	private ErrorEventHandler cbErrorEvent;
	
	public delegate void SendToWallCallbackHandler (bool success);
	public SendToWallCallbackHandler cbSendToWall;

	public delegate void GetFriendsCallbackHandler (bool success, List<Friend> friends = null);
	public GetFriendsCallbackHandler cbGetFriends;

	// Use this for initialization
	public VK() {
		VKApi.OAuthEvent += HandleOAuthEvent;
		VKApi.ApiErrorEvent += HandleApiErrorEvent;
		VKApi.ApiSuccessEvent += HandleApiSuccessEvent;
	}

	~VK() {
		VKApi.OAuthEvent -= HandleOAuthEvent;
		VKApi.ApiErrorEvent -= HandleApiErrorEvent;
		VKApi.ApiSuccessEvent -= HandleApiSuccessEvent;
	}

	private void HandleOAuthEvent(bool success, string error = "", string error_reason = "") {
		VKApi.CloseWebView();
		Debug.LogWarning("OAuth authorization VK: "+success);

		if (cbAuthEvent != null) {
			cbAuthEvent(success, error, error_reason);
		}
		if (cbAuthEventShort != null) {
			cbAuthEventShort(success);
		}
	}

	private void HandleApiErrorEvent(string methodName, string data) {
		Debug.LogWarning("VK.Error! method "+methodName+" DATA: "+data);
		if (cbErrorEvent != null) {
			cbErrorEvent(methodName, data);
		}
		if (methodName.Equals(_reqSendToWall)) {
			if (cbSendToWall != null)
				cbSendToWall(false);
		} else if (methodName.Equals(_reqFriendsGet)) {
			if (cbGetFriends != null)
				cbGetFriends(false);
		}
	}

	private void HandleApiSuccessEvent(string methodName, string data) {
		Debug.LogWarning("VK.Success! method "+methodName+" DATA: "+data);
		if (cbSuccessEvent != null) {
			cbSuccessEvent(methodName, data);
		}
		if (methodName.Equals(_reqSendToWall)) {
			if (cbSendToWall != null)
				cbSendToWall(true);
		} else if (methodName.Equals(_reqFriendsGet) && !friendsIsFull) {
			if (cbGetFriends != null)
				cbGetFriends(true, parseFriendsShortly(data));
		} else if (methodName.Equals(_reqFriendsGet) && friendsIsFull) {
			if (cbGetFriends != null)
				cbGetFriends(true, parseFriendsFully(data));
		}
	}

	//---------------------------------------------------
	private List<Friend> parseFriendsShortly(string data) {
		List<Friend> res = new List<Friend>();

		/* EXAMPLE:
		response: {
		count: 694,
		items: [2, 5, 6]
		}
		*/

		bool success = true;
		var resp = JSON.JsonDecode(data, ref success) as Hashtable;

		if (resp.ContainsKey("response")) {
			var respData = resp["response"] as Hashtable;
			if (respData.ContainsKey("items")) {
				var items = respData["items"] as Array;
				foreach (var currId in items) {
					res.Add(new Friend() {
						id = currId.ToString()
					});
				}
			}
		}


		return res;
	}

	private List<Friend> parseFriendsFully(string data) {
		List<Friend> res = new List<Friend>();
		//TODO: implement it

		/*
		response: {
		count: 694,
		items: [{
		id: 2,
		first_name: 'Александра',
		last_name: 'Владимирова',
		photo_200: 'http://cs614624.v...8/uAwoJS6bdMQ.jpg',
		online: 0
		}, {
		id: 5,
		first_name: 'Илья',
		last_name: 'Перекопский',
		photo_200: 'http://cs623823.v...6/W56sWVPghK4.jpg',
		online: 0
		}, {
		id: 6,
		first_name: 'Николай',
		last_name: 'Дуров',
		photo_200: 'http://cs320116.v...e/EPhF3ro_EeM.jpg',
		online: 0
		}]
		}
		*/

		bool success = true;
		var resp = JSON.JsonDecode(data, ref success) as Hashtable;

		var fields = _reqGetFriends_FullList_Fields.Split(',');
		var propertiesArray = typeof(Friend).GetProperties();
		var properties = new List<System.Reflection.PropertyInfo>();
		properties.AddRange(properties);

		if (resp.ContainsKey("response")) {
			var respData = resp["response"] as Hashtable;
			if (respData.ContainsKey("items")) {
				var items = respData["items"] as Array;
				foreach (var itemData in items) {
					var item = itemData as Hashtable;
					var friend = new Friend();
					foreach (var field in fields) {
						var property = properties.Find(p => { return p.Name.Equals(field); });
						if (item.ContainsKey(field) && property != null) {
							property.SetValue(friend, item[field].ToString(), null);
						}
					}
				}
			}
		}


		return res;
	}

	//---------------------------------------------------


	public void DoLogIn(AuthEventHandler callback = null) {
		cbAuthEvent = callback;
		VKApi.Init();
	}
	public void DoLogIn(AuthEventHandlerShort callback = null) {
		cbAuthEventShort = callback;
		VKApi.Init();
	}

	public void InitApiHandlers(SuccessEventHandler callbackSuccess = null, ErrorEventHandler callbackError = null) {
		cbSuccessEvent = callbackSuccess;
		cbErrorEvent = callbackError;
	}

	public void SendToWall(string text, bool friendsOnly = false, SendToWallCallbackHandler callback = null) {
		cbSendToWall = callback;

		var req = new Hashtable();
		req.Add("friends_only", friendsOnly ? "1" : "0");
		req.Add("message", text);
		VKApi.Api(_reqSendToWall, req);
	}

	//NB! Be sure not to get friends wery quickly! No queues was implemented. Responses may be mixed and wrong.
	bool friendsIsFull = false;

	public void GetShortFriendList(GetFriendsCallbackHandler callback = null, int offset = -1, int count = -1) {
		cbGetFriends = callback;
		friendsIsFull = false;

		var req = new Hashtable();
		req.Add("order", "mobile");
		if (offset >= 0)
			req.Add("offset", offset.ToString());
		if (count >= 0)
			req.Add("count", count.ToString());
		req.Add("fields", _reqGetFriends_ShortList_Fields);
		VKApi.Api(_reqFriendsGet, req);
	}

	public void GetFullFriendList(GetFriendsCallbackHandler callback = null, int offset = -1, int count = -1) {
		cbGetFriends = callback;
		friendsIsFull = true;

		var req = new Hashtable();
		req.Add("order", "mobile");
		if (offset >= 0)
			req.Add("offset", offset.ToString());
		if (count >= 0)
			req.Add("count", count.ToString());
		req.Add("fields", _reqGetFriends_FullList_Fields);
		VKApi.Api(_reqFriendsGet, req);
	}

}
