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
    public Tank SourceTank;   // the tank that fired the shot

    public bool RotatesWithVelocity = true;

    public GameObject ExplosionPrefab;

	// Update is called once per frame
	void Update () {

        // Make sure we're pointing into our direction of travel
        if( RotatesWithVelocity )
        {
            float angle = Mathf.Atan2( rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }

        if( isServer == true )
        {
            // Check for ground collisions, since that's not handled by
            // the physics engine
        }
            
	}

    void OnCollisionEnter2D(Collision2D collision)
    {
        OnTriggerEnter2D( collision.collider );
    }

/*    [ClientRpc]
    void RpcDoExplosion( Vector2 position )
    {
        GameObject go = Instantiate( ExplosionPrefab, position, Quaternion.identity );
        go.GetComponent<BulletExplosion>().Radius = Radius;
    }

*/
    void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("OnTriggerEnter2D");
        if( isServer == false )
        {
            // Only the server resolve explosions.
            return;
        }

        // We have collided with something

        // Is this our own tank?  And should we detonate?
        if(SourceTank != null && SourceTank.GetComponent<Rigidbody2D>() == collider.attachedRigidbody)
        {
            return;
        }

        // If we get here, detonate and hurt everything (including the source tank)
        // within our radius

        // TODO: Explosion Graphic
        //       USE AN FX MANAGER
        //RpcDoExplosion( this.transform.position );
        //FxManager.DoExplosion( myExplosionType, position, Radius )

        // Hurt everything in our radius
        Collider2D[] cols = Physics2D.OverlapCircleAll( this.transform.position, Radius );

        foreach(Collider2D col in cols)
        {

            if( col.attachedRigidbody == null )
            {
                continue;
            }


            Health h = col.attachedRigidbody.GetComponent<Health>();

            if(h != null)
            {
                Debug.Log(col.attachedRigidbody.gameObject.name);

                // CONSIDER:  Maybe we need to use an event queue, to not do the damage
                // until the explosion visuals (and falling) have fully been resolved.

                h.CmdChangeHealth( -Damage );
            }
        }

        // TODO: Remove ground pixels?

        // Remove ourselves from the game
        Destroy(gameObject);
    }
}
