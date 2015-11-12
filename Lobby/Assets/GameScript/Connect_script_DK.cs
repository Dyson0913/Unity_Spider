using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using ConnectModule;
using GameCommon.Model;
using GameScript.parser;
using GameCommon.StateMachine;


public class Connect_script_DK: MonoBehaviour {

	private IConnect _Connector;

	//ui
	public UI_Text _ui_gameround;
	public UI_Text _credit;
	public UI_Text _log;
	public UI_Timer _bet_timer;


	public List<UI_Text> cardlist;
	public List<Button> _btnlist;

	//delegate
	public avalibe _avalibelist;

	private StateMachine _state_m;

	//data 
	private Model _model = Model.Instance;
	private string _state;
	private string _gameround;
	private string _gameid;

	private List<string> playercard;
	private List<string> bankercard;
	private List<string> rivercard;

	private List<int> betamount;



	public int _coinSelect;



	// Use this for initialization
	void Start () {

		Debug.Log ("uuid = "+ "ws://106.186.116.216:8201/gamesocket/token/"+_model.getValue("uuid"));		 
		_log = GameObject.Find ("Log").GetComponent<UI_Text>();
		_ui_gameround = GameObject.Find ("gameround").GetComponent<UI_Text>();

		_avalibelist = GameObject.Find ("avalibe_DK").GetComponent<avalibe>();
		_state_m = new StateMachine ();
		_state_m.state = "None";

		playercard = new List<string> ();
		bankercard = new List<string> ();
		rivercard = new List<string> ();
		betamount = new List<int> ();

		foreach (Button bt in _btnlist) 
		{
			//work aroud
			string idx = bt.name;
			bt.onClick.AddListener(()=>betType(idx));	
		}

		
		foreach (UI_Text text in cardlist) 
		{
			//work aroud
			text.textContent = "";
		}


		_coinSelect = 100;


		_Connector = new websocketModule();
		_Connector.parser = new DK_parser ();
		_Connector.create ("ws://106.186.116.216:8201/gamesocket/token/"+_model.getValue("uuid"));
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
			if( openlist[1] =="1")
			{
				string card = e.pack["player_card_list"];
				playercard = new List<string>(card.Split(','));
				if( playercard.Count ==1)cardlist[0].textContent = playercard[0];
				if( playercard.Count ==2)
				{
					cardlist[0].textContent = playercard[0];
					cardlist[1].textContent = playercard[1];
				}

				card = e.pack["banker_card_list"];
				bankercard = new List<string>(card.Split(','));
				if( bankercard.Count ==1)cardlist[2].textContent = bankercard[0];				
				if( bankercard.Count ==2)
				{
					cardlist[2].textContent = bankercard[0];
					cardlist[3].textContent = bankercard[1];
				}

				card = e.pack["river_card_list"];
				rivercard = new List<string>(card.Split(','));
				if( rivercard.Count ==1)cardlist[4].textContent = rivercard[0];				
				if( rivercard.Count ==2)
				{
					cardlist[4].textContent = rivercard[0];
					cardlist[5].textContent = rivercard[1];
				}

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
				if( playercard.Count == 0)cardlist[0].textContent = e.pack["card_list"];
				if( playercard.Count == 1)cardlist[1].textContent = e.pack["card_list"];
				playercard.Add(e.pack["card_list"]);
			}

			if( cardtype == "Banker")
			{
				if( bankercard.Count == 0)cardlist[2].textContent = e.pack["card_list"];
				if( bankercard.Count == 1)cardlist[3].textContent = e.pack["card_list"];
				bankercard.Add(e.pack["card_list"]);
			}

			if( cardtype == "River")
			{
				if( rivercard.Count == 0)cardlist[4].textContent = e.pack["card_list"];
				if( rivercard.Count == 1)cardlist[5].textContent = e.pack["card_list"];
				rivercard.Add(e.pack["card_list"]);
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



	public void betType(string btnname)
	{
		Debug.Log ("btnname = "+btnname);
	}



	// Update is called once per frame
	void Update () {
	

	}

	void OnDestroy() {
		_Connector.close ();
		Debug.Log ("DK connect destroy");
	}
	
}
