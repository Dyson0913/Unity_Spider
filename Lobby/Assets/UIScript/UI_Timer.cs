using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_Timer : MonoBehaviour {

	public float delta;
	public float delay;
	public string timer_call{ get; set; }

	// Use this for initialization
	void Start () 
	{	

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
