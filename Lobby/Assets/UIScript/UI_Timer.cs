using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_Timer : MonoBehaviour {

	private Text _text;
	public string textContent{ get; set; }
	// Use this for initialization
	void Start () 
	{	
		_text = this.GetComponent<Text>();
		textContent = "";
	}
	
	// Update is called once per frame
	void Update () {
		_text.text = textContent;
	}
}
