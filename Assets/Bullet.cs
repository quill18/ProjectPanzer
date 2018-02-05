using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour {

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
	}

    Rigidbody2D rb;

    public float Radius = 2f;
    public float Damage = 10f;
    public bool DamageFallsOff = true;
    public GameObject SourceTank;   // the tank that fired the shot
	
	// Update is called once per frame
	void Update () {

        // Make sure we're pointing into our direction of travel
        float angle = Mathf.Atan2( rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        rb.rotation = angle;

        if( isServer == true )
        {
            // Check for ground collisions, since that's not handled by
            // the physics engine
        }
            
	}

    void OnTriggerEnter2D(Collider2D collider)
    {
        if( isServer == false )
        {
            // Only the server resolve explosions.
            return;
        }

        // We have collided with something

        // Is this our own tank?  And should we detonate?

        // If we get here, detonate and hurt everything (including the source tank)
        // within our radius
    }
}
