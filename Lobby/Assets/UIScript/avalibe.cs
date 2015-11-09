using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class avalibe : MonoBehaviour {

	public List<GameObject> _list;

	private List<string> data;

	private bool viewUpdate;

	// Use this for initialization
	void Start () {
		viewUpdate = false;
	}

	public void set_avalible(List<string> openlist)
	{
		data = openlist;
		viewUpdate = true;
	}

	private void view_update()
	{
		viewUpdate = false;
		for (int i=0; i< _list.Count; i++) 
		{
			Debug.Log ("open[i] = "+data[i]);
			bool open = false;
			if( data[i] =="1") open = true;
			else open = false;
			
			_list[i].gameObject.SetActive(open);
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
