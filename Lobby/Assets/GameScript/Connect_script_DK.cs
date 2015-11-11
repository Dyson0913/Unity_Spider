using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ConnectModule;
using GameScript.parser;
using GameCommon.StateMachine;


public class Connect_script_DK: MonoBehaviour {

	private IConnect _Connector;

	//ui
	public UI_Text _ui_gameround;
	public UI_Text _credit;
	public UI_Text _log;
	public UI_Timer _bet_timer;

	public UI_Text _pcard1;
	public UI_Text _pcard2;

	public UI_Text _bcard1;
	public UI_Text _bcard2;

	public UI_Text _rcard1;
	public UI_Text _rcard2;

	//delegate
	public avalibe _avalibelist;

	private StateMachine _state_m;
	//data 
	private string _state;
	private string _gameround;
	private string _gameid;


	public string _uuid;

	// Use this for initialization
	void Start () {

		//Debug.Log ("uuid = "+ "ws://106.186.116.216:8201/gamesocket/token/"+_uuid);		 
		_log = GameObject.Find ("Log").GetComponent<UI_Text>();
		_ui_gameround = GameObject.Find ("gameround").GetComponent<UI_Text>();

		_pcard1 = GameObject.Find ("Card_Text_1").GetComponent<UI_Text>();
		_bcard1 = GameObject.Find ("Card_Text_3").GetComponent<UI_Text>();
		_rcard1 = GameObject.Find ("Card_Text_5").GetComponent<UI_Text>();

		_avalibelist = GameObject.Find ("avalibe_DK").GetComponent<avalibe>();
		_state_m = new StateMachine ();
		_state_m.state = "None";

//		List<string> openlist = new List<string> ();
//		openlist.Add ("0");
//		openlist.Add ("0");
//		openlist.Add ("0");
//		_avalibelist.set_avalible(openlist);

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
			List<string> openlist = _state_m.stateupdate(_state);
			_avalibelist.set_avalible(openlist);

			_gameid = e.pack["game_id"];

			if( openlist[0] =="1")
			{
				_gameround = e.pack["game_round"];
				_ui_gameround.textContent = "局號:"+ _gameround;

				//timer
				//_bet_timer.excute();
				//_bet_timer = GameObject.Find ("bet_time").GetComponent<UI_Timer>();
				//_bet_timer.
			}



			Debug.Log("pack all p= "+ e.pack["player_card_list"]);
			Debug.Log("pack all b= "+ e.pack["banker_card_list"]);
			Debug.Log("pack all r= "+ e.pack["river_card_list"]);
			Debug.Log("pack all e= "+ e.pack["extra_card_list"]);
		}
		if (st == "MsgBPState") 
		{
			_state = e.pack["game_state"];
			_log.textContent = _state;
			List<string> openlist = _state_m.stateupdate(_state);
			if( openlist !=null)
			{
				_avalibelist.set_avalible(openlist);
			}

			if( openlist[0] =="1")
			{
				_gameround = e.pack["game_round"];
				_ui_gameround.textContent = "局號:"+ _gameround;

				//timer
				//_bet_timer.excute();
				//_bet_timer = GameObject.Find ("bet_time").GetComponent<UI_Timer>();
			}

		}
		if (st == "MsgBPOpenCard") 
		{
			_state = e.pack["game_state"];
			_log.textContent = _state;

			List<string> openlist = _state_m.stateupdate(_state);
			if( openlist !=null) _avalibelist.set_avalible(openlist);

			Debug.Log("carty = "+e.pack["card_type"]);
			Debug.Log("card_list = "+e.pack["card_list"]);
			string cardtype = e.pack["card_type"];
			if( cardtype == "Player")
			{
				_pcard1.textContent = e.pack["card_list"];
			}

			if( cardtype == "Banker")
			{
				_bcard1.textContent = e.pack["card_list"];
			}

			if( cardtype == "River")
			{
				_rcard1.textContent = e.pack["card_list"];
			}

		}
		if (st == "MsgBPEndRound") 
		{
			_state = e.pack["game_state"];
			_log.textContent = _state;
			Debug.Log("carty = "+e.pack["bet_type"]);
			List<string> openlist = _state_m.stateupdate(_state);
			_avalibelist.set_avalible(openlist);
		}
		if (st == "check") 
		{
			Debug.Log("pack all = "+ e.pack["_all"]);
		}
	}

	public void newRound()
	{

	}



	// Update is called once per frame
	void Update () {
	

	}

	void OnDestroy() {
		_Connector.close ();
		Debug.Log ("DK connect destroy");
	}
	
}
