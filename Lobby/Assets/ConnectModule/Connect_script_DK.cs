using UnityEngine;
using System.Collections;

using System.Collections.Generic;



using ConnectModule;

public class Connect_script_DK: MonoBehaviour {

	private IConnect _Connector;

	public UI_Text _name;
	public UI_Text _credit;
	public UI_Text _log;
	public avalibe _avalibelist;

	private string _state;
	private string _gameround;
	private string _gameid;

	public string _uuid;

	// Use this for initialization
	void Start () {

		//Debug.Log ("uuid = "+ "ws://106.186.116.216:8201/gamesocket/token/"+_uuid);		 
		_log = GameObject.Find ("Log").GetComponent<UI_Text>();
		_Connector = new websocketModule();
		_Connector.parser = new DK_parser ();
		_Connector.create ("ws://106.186.116.216:8201/gamesocket/token/"+_uuid);
		_Connector.MsgResponse += OnMessage;
		_Connector.stateResponse += Onstate;
		_Connector.connect ();
	}

	private void Onstate(object sender,stringArgs e)
	{
		Debug.Log("DK Onstate = "+ e.msg);
	}

	private void OnMessage(object sender,packArgs e)
	{
		string st = e.pack ["message_type"];
		if (st == "MsgBPInitialInfo") 
		{
			_state = e.pack["game_state"];
			_log.textContent = _state;
			_gameround = e.pack["game_round"];
			_gameid = e.pack["game_id"];
			Debug.Log("pack all p= "+ e.pack["player_card_list"]);
			Debug.Log("pack all b= "+ e.pack["banker_card_list"]);
			Debug.Log("pack all r= "+ e.pack["river_card_list"]);
			Debug.Log("pack all e= "+ e.pack["extra_card_list"]);
		}
		if (st == "MsgBPState") 
		{
			_state = e.pack["game_state"];
			_log.textContent = _state;

		}
		if (st == "MsgBPOpenCard") 
		{
			_state = e.pack["game_state"];
			_log.textContent = _state;
			Debug.Log("carty = "+e.pack["card_type"]);
		}
		if (st == "MsgBPEndRound") 
		{
			_state = e.pack["game_state"];
			_log.textContent = _state;
			Debug.Log("carty = "+e.pack["bet_type"]);
		}
		if (st == "check") 
		{
			Debug.Log("pack all = "+ e.pack["_all"]);
		}
	}

	public void click1()
	{
		Debug.Log ("click1");	
		_log.textContent = "clieck1";
	}

	public void click2()
	{
		Debug.Log ("click2");	
		_log.textContent = "clieck2";
	}

	public void click3()
	{
		Debug.Log ("click3");	
		_log.textContent = "clieck3";
		string s = "0,0,0,0";
		_avalibelist.set_avalible(new List<string>(s.Split(',')));
		Application.LoadLevel("DK");
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
		Debug.Log ("DK connect destroy");
	}
	
}
