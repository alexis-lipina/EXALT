using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class HideTitle : MonoBehaviour
{
    Player _player;
    bool isGoingAway = false;
    float timer = 0.2f;

	// Use this for initialization
	void Start ()
    {
        _player = ReInput.players.GetPlayer(0);
    }

    // Update is called once per frame
    void Update ()
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            if (_player.GetAnyButton())
            {
                isGoingAway = true;
            }
            if (isGoingAway)
            {
                GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, Mathf.Lerp(GetComponent<SpriteRenderer>().color.a, 0, 0.2f));
            }
        }
		
	}
}
