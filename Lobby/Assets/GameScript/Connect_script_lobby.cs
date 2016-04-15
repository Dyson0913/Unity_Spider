using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using Facebook.Unity;
using Facebook.MiniJSON;

using ConnectModule;
using GameCommon.Model;
using GameScript.parser;
using ProgressBar;



public class Connect_script_lobby : MonoBehaviour {

	private IConnect _Connector;

	public UI_Text _name;
	public UI_Text _credit;
	public UI_Text _log;
	public avalibe _avalibelist;
	public ProgressRadialBehaviour _progressbar;

	public GameObject _bg;
	private Sprite _mybg;

	public avalibe _msghint;

	public List<Button> _gameIcon;
	public List<Sprite> _bglist;
	public UI_Enable _shareitem;

	public Connect_script_DK _dk_proxy;

	private permission_handler _permission_handler;


	private Model _model = Model.Instance;
	
	// Use this for initialization
	void Start () {

		_shareitem.item_init ();
		string s = "0,0,0";
		_msghint.set_avalible (new List<string> (s.Split (',')));

		foreach (Button bt in _gameIcon) 
		{
			//work aroud
			string idx = bt.name;
			bt.onClick.AddListener(()=>IconClick(idx));	
		}

		this.login ();
	}

	private void login()
	{
		_Connector = new websocketModule();
		_Connector.parser = new lobby_parser ();
		_Connector.create ("ws://www.mm9900.net:8001/gamesocket/token/6f4922f45568161a8cdf4ad2299f6d23");
		_Connector.MsgResponse += OnMessage;
		_Connector.stateResponse += Onstate;
		
		_Connector.connect ();
	}


	private void Onstate(object sender,stringArgs e)
	{
		_progressbar.Value = 100;
		Debug.Log("Lobby Onstate = "+ e.msg);
	}

	private void OnMessage(object sender,packArgs e)
	{
		//Debug.Log ("Lobby message = " + e.pack ["message_type"]);
		string state = e.pack ["message_type"];
		if (state == "MsgLogin") {
			_name.textContent = e.pack ["player_name"];
			_credit.textContent = e.pack ["player_credit"];
			_model.putValue ("uuid", e.pack ["player_uuid"]);
			string s = e.pack ["game_avaliable"];
			_model.putValue ("game_list", s);

			_avalibelist.set_avalible (new List<string> (s.Split (',')));

		} else if (state == "MsgKeepLive") {

		} 
		else if (state == "MsgPlayerCreditUpdate") 
		{
			_credit.textContent = e.pack ["player_credit"];
		}
		if (e.pack ["message_type"] == "check") 
		{
			Debug.Log("pack all = "+ e.pack["_all"]);
		}

		//TODO game msg send to here later version
		// game_type == game ,pass to game
		//if (e.pack ["message_type"] == "MsgBPInitialInfo") {
		//	 if (e.pack["game_type"] =="pa") {

		//	}
		//	if (e.pack["game_type"] =="pa"){

		//	}
		//}
	}

	public void Log(string log)
	{
		_log.textContent =log;
	}

	private void map_bg(string game_game)
	{
		if (game_game == "pa") {
			_mybg = _bglist [2];
		} else if (game_game == "dk") {
			_mybg = _bglist [1];
		} else if (game_game == "bingo") {
			_mybg = _bglist [4];
		} else if (game_game == "s7pk") {
			_mybg = _bglist [3];
		} 
		_model.putValue ("current_game", game_game);

	}

	public void IconClick(string btnname)
	{
		//Debug.Log ("value = " + btnname);
		map_bg (btnname);
		icon_forbidden ();
		fade_out ();
	}

	private void icon_forbidden ()
	{
		string s = "0,0,0,0,0";
		_avalibelist.set_avalible(new List<string>(s.Split(',')));
	}

	private void fade_out()
	{
		_bg.GetComponent<Image> ().CrossFadeAlpha(0.1f, 0.5f, false);
		Invoke("lobbytogame", 0.5f);
	}

	public void lobbytogame()
	{
		_bg.GetComponent<Image> ().sprite = _mybg;
		_bg.GetComponent<Image> ().CrossFadeAlpha(1f, 0.5f, false);
		Invoke("loadgameok", 0.5f);
	}

	public void loadgameok()
	{
		_shareitem.item_set ("into_game");

		//each game handle
		string mygame = _model.getValue ("current_game");
		Debug.Log ("into "+ mygame);
		if (mygame == "dk") {
			_dk_proxy.init();
		}
		if (mygame == "pa") {
			
		}
	}


	public void back_lobby()
	{
		_bg.GetComponent<Image> ().CrossFadeAlpha(0.1f, 0.5f, false);
		_mybg = _bglist [0];
		Invoke("game_back_lobby", 0.5f);

		_shareitem.item_set ("back_lobby");


		
		//handle leaveing game
		string mygame = _model.getValue ("current_game");
		Debug.Log ("out "+ mygame);
		if (mygame == "dk") {
			_dk_proxy.leave();
		}
		if (mygame == "pa") {
			
		}
	}

	public void game_back_lobby()
	{
		_bg.GetComponent<Image> ().sprite = _mybg;
		_bg.GetComponent<Image> ().CrossFadeAlpha(1f, 0.5f, false);
		Invoke("back_lobby_ok", 0.5f);
	}

	public void back_lobby_ok()
	{
		string s = _model.getValue("game_list");
		_avalibelist.set_avalible(new List<string>(s.Split(',')));
	}

	public void loading_down()
	{
		Debug.Log ("loading_down");
		_progressbar.gameObject.SetActive (false);
	}

	public void empty_test()
	{
		Debug.Log ("empty_test");
	}

	public void click_Fb_login()
	{
		FB.Init (this.OnInitComplete, this.OnHideUnity);

	}

	private void OnInitComplete()
	{
		string logMessage = string.Format(
			"OnInitCompleteCalled IsLoggedIn='{0}' IsInitialized='{1}'",
			FB.IsLoggedIn,
			FB.IsInitialized);
		LogView.AddLog(logMessage);

		//登入流程
		//if (!FB.IsLoggedIn) {
			this.CallFBLogin ();
		//}
	}

	private void CallFBLogin()
	{
		_permission_handler = new permission_handler ();
		_permission_handler.call_back = this.Fb_data_ok;
		FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, _permission_handler.result_handle);
	}

	private void Fb_data_ok (params object[] args)
	{
		LogView.AddLog("Fb_data_ok");
		_name.textContent = args[0].ToString();

		this.login();
	}

	private void OnHideUnity(bool isGameShown)
	{
		//this.Status = "Success - Check logk for details";
		//this.LastResponse = string.Format("Success Response: OnHideUnity Called {0}\n", isGameShown);
		//LogView.AddLog("Is game shown: " + isGameShown);
	}



	// Update is called once per frame
	void Update () {
	
		//TODO isolate to Independon script
		if (Input.GetKeyUp (KeyCode.Escape)) 
		{
			Application.Quit();
		}

		if (!_progressbar.isDone) {
			_progressbar.Value = _progressbar.Value + 1;
			Debug.Log ("new value: " + _progressbar.Value);
		} 
	}

	void OnDestroy() {
		_Connector.close ();
		Debug.Log ("Lobby connect destroy");
	}
	
}
