using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_Timer : MonoBehaviour {

	public float delta;
	public float delay;
	public string timer_call{ get; set; }
	public GameObject needTimerob;

	// Use this for initialization
	void Start () 
	{	
		needTimerob = this.gameObject;
		timer_call = needTimerob.GetComponent<UI_Text> ().timer_call;
	}

	public void excute()
	{
		InvokeRepeating (timer_call, delay, delta);
	}

	public void stop()
	{
		CancelInvoke (timer_call);
	}
	
}
