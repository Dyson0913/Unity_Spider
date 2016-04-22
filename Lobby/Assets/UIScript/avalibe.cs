using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using System;
using System.Reflection;

public class avalibe : MonoBehaviour {

	public List<GameObject> _list;

	private List<string> data;

	private bool viewUpdate;

	public delegate void cutomer_fun(int x,bool b);

	public cutomer_fun my_cus;

	public enum effect
	{
		just_open_close=0,
		fadeout =1
	}

	// Use this for initialization
	void Start () {
		viewUpdate = false;
		data = new List<string> ();
		my_cus = null;
	}

	public void auto_set_avalible(int enable_item_idx,effect f)
	{
		data.Clear ();
		if( f == effect.just_open_close) my_cus = myavalible;
		if( f == effect.fadeout) my_cus = myfadeout;

		int k = _list.Count;
		string a = Convert.ToString (enable_item_idx, 2);
		string full = string.Format("{0:D"+k+"}",Int32.Parse(a));
		char[] ns = full.ToCharArray ();
		Array.Reverse(ns);
		for (int i=0; i< ns.Length; i++) {
			data.Add ( ns[i].ToString());
		}


		viewUpdate = true;

	}

	public void myfadeout(int idx,bool b)
	{

		_list[idx].gameObject.SetActive(b);
		if (b == false)
			return;

		Image a = _list [idx].gameObject.GetComponent<Image> ();
		Color c = a.color;
		c.a = 1;
		_list[idx].gameObject.GetComponent<Image> ().CrossFadeAlpha (0.0f, 1.5f, false);
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
