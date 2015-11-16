using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_Text : MonoBehaviour {

	public Text _text {get; set; }
	public string textContent{ get; set; }

	public bool countDown{ get; set; }
	// Use this for initialization
	void Start () 
	{	
		_text = this.GetComponent<Text>();
		countDown = false;
	}
	
	// Update is called once per frame
	void Update () {
		_text.text = textContent;

		if( countDown)
		{
			countDown = false;
			count_douwn();
		}
	}

	public void count_douwn()
	{
		InvokeRepeating("sub", 0, 1);
	}

	public void sub()
	{
		int cnt = int.Parse(textContent);

		cnt -= 1;
		if (cnt == -1)
		{
			textContent = "";
			CancelInvoke();
			return;
		}
		textContent = cnt.ToString ();
	}
}
