using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//websocket
using WebSocketSharp;

//json
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class tw : MonoBehaviour {
	
	public GameObject cave;
	private JObject jo = new JObject();

	public TextMesh mesh;

	public Text name;
	public Text credit;
	public Text uilog;

	private string _name;
	private string _credit;
	private string _UIlog = "";

	private WebSocket _ws;

	private bool loginOk = false;

	// Use this for initialization
	void Start () {
		Debug.Log ("aaa");
		cave.gameObject.SetActive (false);
		loginOk = false;
		Invoke("connect", 1.0f);
		_UIlog = "start";
	}

	public void connect()
	{
		_UIlog = "connect";
		_ws = new WebSocket ("ws://106.186.116.216:8001/gamesocket/token/c9f0f895fb98ab9159f51fd0297e236d");
		_ws.OnMessage += (sender, e) => {
				_UIlog += e.Data +"\r\n";
				Debug.Log ("onMessage says: " + e.Data);
				Debug.Log("json ob 1"+ e.Data);				
				jo = JsonConvert.DeserializeObject<JObject>(e.Data);
				Debug.Log("json ob 1 = "+jo.Property("player_info").Value);

				//TODO level 2 how to split
				JObject jo2 = new JObject();
				jo2 = JsonConvert.DeserializeObject<JObject>(jo.Property("player_info").Value.ToString());
				Debug.Log("json ob 2 = "+jo2.Property("player_name").Value);
				_name = jo2.Property("player_account").Value.ToString();
				_credit = jo2.Property("player_credit").Value.ToString();

				loginOk = true;

			};
		_ws.OnClose += (sender, e) => {
				Debug.Log ("ws close" + e.Code);
				_UIlog += "close"+ e.Code + "\r\n";
				if( e.Code != 1001)_UIlog = "ws close =" +e.Code;
			};
		_ws.OnError += (sender, e) => {
				Debug.Log ("ws error" + e.Exception);
				_UIlog += "ws error =" +e.Exception.ToString() +"\r\n";
			};
		_ws.OnOpen += (sender, e) => {
				Debug.Log ("ws open");
				_UIlog += "open"+ "\r\n";
			};
		
		_ws.Connect ();
			//ws.ConnectAsync ();
			//ws.Send ("BALUS");

	}

	// Update is called once per frame
	void Update () {
		uilog.text = _UIlog;
		if( loginOk)
		{
			loginOk = false;
			cave.gameObject.SetActive(true);
			name.text = _name;
			credit.text = _credit;
			//_UIlog = "";
			mesh.text = _credit;
		}
	}

	void OnDestroy() {
		_ws.Close ();
		Debug.Log ("destroy");
	}


}
