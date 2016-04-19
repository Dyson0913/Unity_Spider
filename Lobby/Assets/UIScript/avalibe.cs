using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Reflection;

public class avalibe : MonoBehaviour {

	public List<GameObject> _list;

	private List<string> data;

	private bool viewUpdate;

	public delegate void cutomer_fun(int x,bool b);

	public cutomer_fun my_cus;

	// Use this for initialization
	void Start () {
		viewUpdate = false;
		data = new List<string> ();
		my_cus = null;
	}

	public void auto_set_avalible(int enable_item_idx,cutomer_fun f)
	{
		data.Clear ();
		my_cus = f;
		for (int i=0; i< _list.Count; i++) {
			if( enable_item_idx == i) data.Add("1");
			else data.Add("0");
		}
		viewUpdate = true;

	}

	public void fadeout(int idx,bool b)
	{
		GameObject ob = _list [0];


		//_list[idx].gameObject.SetActive(b);
		//_list[0].gameObject.GetComponent<Image ().CrossFadeAlpha (1f, 0.5f, false);
	}


	public void set_avalible(List<string> openlist)
	{
		data = openlist;
		my_cus = this.myavalible;
		viewUpdate = true;
	}

	public void myavalible(int idx,bool b)
	{
	  _list[idx].gameObject.SetActive(b);
	}
	

	private void view_update()
	{
		viewUpdate = false;
		for (int i=0; i< _list.Count; i++) 
		{
			bool open = false;
			if( data[i] =="1") open = true;
			else open = false;

			//customer
			my_cus(i,open);

		}

		viewUpdate = false;
	}

	// Update is called once per frame
	void Update () {
		if (viewUpdate) 
		{
			view_update();
		}
	}
}
