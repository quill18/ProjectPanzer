using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletExplosion : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
    public float Radius = 1;
    float fadeInTime = 0.25f, fadeInTimeLeft = 0.25f;
    float fadeOutTime = 0.5f, fadeOutTimeLeft = 0.5f;

    public GameObject CoreGraphic;

	// Update is called once per frame
	void Update () {
        if(fadeInTimeLeft > 0)
        {
            fadeInTimeLeft = Mathf.Max( fadeInTimeLeft - Time.deltaTime, 0);
            float r = Radius * (1 - fadeInTimeLeft/fadeInTime);
            CoreGraphic.transform.localScale = Vector3.one * r;

            return;
        }

        // TODO: Remove ground pixel

        if(fadeOutTimeLeft > 0)
        {
            fadeOutTimeLeft = Mathf.Max( fadeOutTime - Time.deltaTime, 0);
            Color c = CoreGraphic.GetComponent<SpriteRenderer>().color;
            c.a = (fadeOutTimeLeft/fadeOutTime);
            return;
        }

        // If we get here, all the fade in/out animation is done.  
        // We can destroy ourselves, unless we have particle children -- TODO

        Destroy(gameObject);
	}
}
