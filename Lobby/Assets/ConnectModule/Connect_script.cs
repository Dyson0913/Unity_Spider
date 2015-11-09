using UnityEngine;
using System.Collections;

using System.Collections.Generic;



using ConnectModule;

public class Connect_script : MonoBehaviour {

	private IConnect _Connector;

	public UI_Text _name;
	public UI_Text _credit;
	public UI_Text _log;
	public avalibe _avalibelist;

	// Use this for initialization
	void Start () {
		_Connector = websocketModule.Instance;
		_Connector.create ();
		_Connector.MsgResponse += OnMessage;

		//TODO isolate to Independon script
		DontDestroyOnLoad(this);
		_Connector.connect ();
	}


	private void OnMessage(object sender,packArgs e)
	{
		Debug.Log ("message = " + e.pack ["message_type"]);
		if (e.pack ["message_type"] == "MsgLogin") 
		{
			_name.textContent = e.pack["player_name"];
			_credit.textContent = e.pack["player_credit"];
			Debug.Log("pack all = "+ e.pack["game_avaliable"]);
			string s = e.pack["game_avaliable"];
			//s.Split(',').toLi
			_avalibelist.set_avalible(new List<string>(s.Split(',')));

		}
		if (e.pack ["message_type"] == "check") 
		{
			Debug.Log("pack all = "+ e.pack["_all"]);
		}
	}

	// Update is called once per frame
	void Update () {
	
		//TODO isolate to Independon script
		if (Input.GetKeyUp (KeyCode.Escape)) 
		{
			Application.Quit();
		}
	}

	void OnDestroy() {
		_Connector.close ();
		Debug.Log ("destroy");
	}
	
}
