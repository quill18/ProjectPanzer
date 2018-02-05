using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

	// Use this for initialization
	void Start () {
        // NOTE: Start() runs even before anyone connects to any server
	}

    [SyncVar]
    public float TimeLeft = 180;
	
	// Update is called once per frame
	void Update () {
		
        if(isServer == false)
        {
            return;
        }

        // FOR NOW -- we just reset the map/players every 3 minutes.
        TimeLeft -= Time.deltaTime;
        if(TimeLeft <= 0)
        {
            // Restart the match
        }

	}


}
