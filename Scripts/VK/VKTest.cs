using UnityEngine;
using System.Collections;

public class VKTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var vk = new VK();
		vk.DoLogIn((success) => {
			Debug.LogWarning("====================> VK.Login: "+success);
			vk.SendToWall("Testing mobile app", true, success => {
				Debug.LogWarning("====================> VK.SendToWall: "+success);
			});
			vk.GetShortFriendList((success, friends) => {
				Debug.LogWarning("====================> VK.GetShortFriendList: "+success);
				foreach (var fr in friends) {
					Debug.LogWarning("FriendID: "+fr.id);
				}
			});
		});
	}

}
