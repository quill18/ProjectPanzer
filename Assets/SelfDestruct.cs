using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}

    public float Timer = 5;
	
	// Update is called once per frame
	void Update () {
        // TODO: Do we need to check the game's pause state?
        Timer -= Time.deltaTime;

        if(Timer <= 0)
        {
            Destroy(gameObject);
        }
	}
}
