using UnityEngine;
using System.Collections;
using System;
using ConnectModule;


public class UI_Image : MonoBehaviour {

	private int rotateType = 0;

	private int _identyfy = 0;

	public event EventHandler<stringArgs> CardRotateComplete ;
	public void On_complete(stringArgs state)
	{
		if (CardRotateComplete != null) 
		{
			CardRotateComplete(this, state);
		}
	}

	private Vector3 _ClockWise = new Vector3(0, 5, 0);
	private Vector3 _CountClockWise = new Vector3(0, -5, 0);
	// Use this for initialization
	void Start () {
	
	}

	public void rotate_to(float x, float y,float z)
	{

		//two way to set 
		this.gameObject.transform.localEulerAngles = new Vector3(x,y,z);
		//this.gameObject.transform.localRotation = Quaternion.Euler(x,y,z);

		//set world rotation
		//this.gameObject.transform.localRotation = Quaternion.Euler(x,y,z);
	}

	void FixedUpdate () {

		if (rotateType == 1) {
			//Debug.Log("y = "+this.gameObject.transform.eulerAngles.y);
			if ( this.gameObject.transform.eulerAngles.y > 90) {
				rotateType =0;
				On_complete(new stringArgs (_identyfy.ToString()));
			}
			else this.gameObject.transform.Rotate(_ClockWise);
		}
		if (rotateType == 2) {
			if ( this.gameObject.transform.eulerAngles.y +_ClockWise.y >360) {
				rotateType =0;
				On_complete(new stringArgs (_identyfy.ToString()));
			}
			else this.gameObject.transform.Rotate(_ClockWise);
		}

	}

	public void RotateCard(int rotype,int id)
	{
		rotateType = rotype;
		_identyfy = id;
	}
}
