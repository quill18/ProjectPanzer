using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Tank : NetworkBehaviour {

    // This script will run on ALL clients AND on the server
    // Additionally, one of the clients may be the local authority

	// Use this for initialization
	void Start () {
		
	}

    // SyncVars?
    float MovementPerTurn = 5;
    float MovementLeft;

    float Speed = 5;

    public GameObject CurrentBulletPrefab;

    public Transform BulletSpawnPoint;

    float turretAngle = 45f;
    float turretPower = 10f;

    [SyncVar]
    Vector3 serverPosition;

    Vector3 serverPositionSmoothVelocity;

    void NewTurn()
    {
        // Runs on server? 
        MovementLeft = MovementPerTurn;
    }
	
	// Update is called once per frame
	void Update () {
		
        if( isServer )
        {
            // Maybe we need to do some server-specific checking/maintenance?
            // Example: Have the tank take ongoing damage
        }

        if( hasAuthority )
        {
            // This is MY object.  I can do whatever I want with it and the network
            // will listen.
            AuthorityUpdate();
        }

        // Do generic updates for ALL clients/server -- like animating movements and such

        // Are we in the correct position?
        if( hasAuthority == false )
        {
            // We don't directly own this object, so we had better move to the server's
            // position.

            transform.position = Vector3.SmoothDamp( 
                transform.position, 
                serverPosition, 
                ref serverPositionSmoothVelocity, 
                0.25f );
        }

	}

    void AuthorityUpdate()
    {

        // Listen for keyboard commands for movement
        float movement = Input.GetAxis("Horizontal") * Speed * Time.deltaTime;

        // TODO: track movement left

        // We have authority, and we don't want any input lag -- so lets move ourselves.
        transform.Translate( movement, 0, 0 );

        // Do we manually tell the network where we moved?
        CmdUpdatePosition( transform.position );


        if(Input.GetKeyUp(KeyCode.Space))
        {
            Vector2 velocity = new Vector2( 
                turretPower * Mathf.Cos( turretAngle * Mathf.Deg2Rad ),
                turretPower * Mathf.Sin( turretAngle * Mathf.Deg2Rad )
            );
            Debug.Log(velocity);

            CmdFireBullet( BulletSpawnPoint.position, velocity );
        }

        // 

    }

    [Command]
    void CmdFireBullet( Vector2 bulletPosition, Vector2 velocity )
    {
        // TODO: Make sure the position and velocity are legal

        float angle = Mathf.Atan2( velocity.y, velocity.x) * Mathf.Rad2Deg;

        // Create the bullet for the clients
        GameObject go = Instantiate(CurrentBulletPrefab, 
            bulletPosition, 
            Quaternion.Euler(0, 0, angle)
        );

        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        rb.velocity = velocity;

        NetworkServer.Spawn( go );
    }

    [Command]
    void CmdUpdatePosition( Vector3 newPosition )
    {
        // TODO: Check to make sure this move is totally legal,
        // both in term of landscape and movement remaining
        // If an illegal move is spotted, do something like:
        //      RpcFixPosition( serverPosition )
        // and return

        serverPosition = newPosition;
    }

    [ClientRpc]
    void RpcFixPosition( Vector3 newPosition )
    {
        // We've received a message from the server to immediately
        // correct this tank's position.
        // This is probably only going to happen if the client tried to 
        // move in some kind of illegal manner.

        transform.position = newPosition;

    }

}
