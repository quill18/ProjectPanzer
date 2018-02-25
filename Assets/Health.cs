using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Health : NetworkBehaviour {

	// Use this for initialization
	void Start () {
        if(isServer)
        {
            hitpoints = 100;
        }
	}

    [SyncVar]
    float hitpoints;
	
	// Update is called once per frame
	void Update () {
		
	}

    public float GetHitpoints()
    {
        return hitpoints;
    }

    [Command]
    public void CmdChangeHealth( float amount )
    {
        hitpoints += amount;

        if(hitpoints <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if( isServer == false )
        {
            Debug.LogError("Client called die!");
            return;
        }

        Debug.Log("DIE!");
        Destroy(gameObject);
    }
}
