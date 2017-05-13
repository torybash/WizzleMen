using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Goal : LevelObject {

	[SerializeField]
	private CanvasGroup playersInPanel;
	[SerializeField]
	private Text playersInText;

	private List<int> _playersIn = new List<int>();


	private void UpdatePlayersInPanel()
	{
		playersInPanel.alpha = _playersIn.Count == 0 ? 0f : 1f;
		playersInText.text = _playersIn.Count + " / 2" ; //TODO!
	}


	void OnTriggerEnter2D(Collider2D other)
	{
		var playerObj = other.GetComponent<WizzPlayer>();
		if (playerObj){
			_playersIn.Add(playerObj.PlayerIdx);
			UpdatePlayersInPanel();
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		var playerObj = other.GetComponent<WizzPlayer>();
		if (playerObj){
			_playersIn.Remove(playerObj.PlayerIdx);
			UpdatePlayersInPanel();
		}
	}
}
