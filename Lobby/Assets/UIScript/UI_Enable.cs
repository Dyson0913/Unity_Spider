using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI_Enable : MonoBehaviour {

	public List<GameObject> _ItemList;

	public void item_init()
	{
		for (int i=0; i< _ItemList.Count; i++) {
			_ItemList[i].gameObject.SetActive (false);
		}
	}

	public void item_set(string state)
	{
		if (state == "into_game") {
			for( int i=0;i< _ItemList.Count;i++)
			{
				_ItemList[i].gameObject.SetActive (true);			
			}
		}
		if (state == "back_lobby") {
			for( int i=0;i< _ItemList.Count;i++)
			{
				_ItemList[i].gameObject.SetActive (false);			
			}
		}
	}

}
