using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
    // This script represents a connected player, who might "own" other objects
    // in the game.

	// Use this for initialization
	void Start () {
		
        // FOR NOW -- we're going to spawn a player's tank as soon as they
        // connnect.  And when a player dies, they will respawn in 3 second.

        if( isServer == true )
        {
            SpawnTank();
        }
	}

    public GameObject TankPrefab;
    GameObject myTank;

	// Update is called once per frame
	void Update () {
		
	}

    public void SpawnTank()
    {
        if( isServer == false )
        {
            Debug.LogError("SpawnTank: Can only do what it's supposed to, from the SERVER!");
            return;
        }

        // This gets called by the game manager when a new round starts
        // and a player needs a tank.

        // Instantiate only creates the object on the LOCAL computer.  It is
        // NOT sent to anyone else in the game.
        myTank = Instantiate(TankPrefab);

        // The way to tell everyone to "spawn" the object in a network-linked fashion,
        // is this:

        NetworkServer.SpawnWithClientAuthority( myTank, connectionToClient );

        // TODO: This player might have a favorite colour/logo, so consider
        // customizing their tank.  Also: Username?
    }
}
