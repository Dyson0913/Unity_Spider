using UnityEngine;
using System.Collections;





using ConnectModule;

public class Connect_script : MonoBehaviour {

	private IConnect _Connector;

	public UI_Text _name;
	public UI_Text _credit;

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
		Debug.Log ("onmessage" + e.pack ["message_type"]);
		if (e.pack ["message_type"] == "MsgLogin") 
		{
			_name.textContent = e.pack["player_name"];
			_credit.textContent = e.pack["player_credit"];
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
	
}
