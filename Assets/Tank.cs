using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Tank : NetworkBehaviour {

    // This script will run on ALL clients AND on the server
    // Additionally, one of the clients may be the local authority

	// Use this for initialization
	void Start () {
        gameManager = GameObject.FindObjectOfType<GameManager>();
	}

    GameManager gameManager;

    // SyncVars?
    float MovementPerTurn = 5;
    float MovementLeft;

    float Speed = 5;
    float TurretSpeed = 180; // Degrees per second
    float TurretPowerSpeed = 10;

    public GameObject CurrentBulletPrefab;
    public Transform TurretPivot;

    public Transform BulletSpawnPoint;

    [SyncVar (hook="OnTurretAngleChange")]
    float turretAngle = 90f;

    float turretPower = 10f;

    [SyncVar]
    Vector3 serverPosition;

    Vector3 serverPositionSmoothVelocity;

    static public Tank LocalTank { get; protected set; }

    public bool IsLockedIn { get; protected set; }

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

            LocalTank = this;

            AuthorityUpdate();
        }

        // Do generic updates for ALL clients/server -- like animating movements and such
        TurretPivot.localRotation = Quaternion.Euler( 0, 0, turretAngle );


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
        AuthorityUpdateMovement();
        AuthorityUpdateAiming();


        // TODO: Make the power display cooler and de-couple from this code
        GameObject pn_go = GameObject.Find("Power Number"); // This is slow!
        pn_go.GetComponent<UnityEngine.UI.Text>().text = turretPower.ToString("#.00");

    }

    void AuthorityUpdateMovement()
    {
        if (IsLockedIn == true || gameManager.TankCanMove(this) == false)
        {
            return;
        }

        // Listen for keyboard commands for movement
        float movement = Input.GetAxis("Horizontal") * Speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            movement *= 0.1f;
        }

        // TODO: track movement left

        // We have authority, and we don't want any input lag -- so lets move ourselves.
        transform.Translate(movement, 0, 0);

        // Do we manually tell the network where we moved?
        CmdUpdatePosition(transform.position);

        if (Input.GetKeyUp(KeyCode.Space))
        {
            // Lock in our movement
            IsLockedIn = true;
            // and let the server know
            CmdLockIn();
        }
    }


    void AuthorityUpdateAiming()
    {
        if ( IsLockedIn == true || gameManager.TankCanAim(this) == false )
        {
            return;
        }

        // ANGLE
        float turretMovement = Input.GetAxis("TurretHorizontal") * TurretSpeed * Time.deltaTime;
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            turretMovement *= 0.1f;
        }

        turretAngle = Mathf.Clamp( turretAngle + turretMovement, 0, 180 );
        CmdSetTurretAngle(turretAngle);

        // POWER
        float powerChange = Input.GetAxis("Vertical") * TurretPowerSpeed * Time.deltaTime;
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            powerChange *= 0.1f;
        }

        turretPower = Mathf.Clamp( turretPower + powerChange, 0, 20 );

        if(Input.GetKeyUp(KeyCode.Space))
        {
            // Lock in our shot
            IsLockedIn = true;
            // and let the server know
            CmdLockIn();


            //Vector2 velocity = new Vector2( 
            //    turretPower * Mathf.Cos( turretAngle * Mathf.Deg2Rad ),
            //    turretPower * Mathf.Sin( turretAngle * Mathf.Deg2Rad )
            //);
            //Debug.Log(velocity);

            //CmdFireBullet( BulletSpawnPoint.position, velocity );
        }

    }

    [Command]
    void CmdLockIn()
    {
        IsLockedIn = true;
    }

    [Command]
    void CmdSetTurretAngle( float angle )
    {
        turretAngle = angle;
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
        go.GetComponent<Bullet>().SourceTank = this;

        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        rb.velocity = velocity;

        NetworkServer.Spawn( go );
    }

    [Command]
    void CmdUpdatePosition( Vector3 newPosition )
    {
        // TODO: Check to make sure this move is totally legal,
        // both in term of landscape and movement remaining
        // and finally (and most importantly) the TURN PHASE
        // If an illegal move is spotted, do something like:
        //      RpcFixPosition( serverPosition )
        // and return

        if( gameManager.TankCanMove( this ) == false )
        {
            // According to the server, this tank should not be allowed
            // to move right now.  DO SOMETHING
        }

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

    [ClientRpc]
    void RpcNewTurn()
    {
        // A new turn has just started

    }


    [ClientRpc]
    public void RpcNewPhase()
    {
        // A new phase has just started

        IsLockedIn = false;
    }


    //  SYNCVAR HOOKS

    void OnTurretAngleChange( float newAngle )
    {
        if( hasAuthority )
        {
            // This is my tank, and my turret -- I can ignore the sync from the server
            return;
        }

        turretAngle = newAngle;
    }



}
