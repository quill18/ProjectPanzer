using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour {

    void Start()
    {
        text = GetComponent<Text>();
    }

    Text text;
    Health health;
	
	void Update () {
		
        if( health == null )
        {
            // We need to find our player's tank
            if( Tank.LocalTank != null )
            {
                health = Tank.LocalTank.GetComponent<Health>();
            }

            if(health == null)
            {
                text.text = "DEAD";
                return;
            }
        }

        text.text = health.GetHitpoints().ToString("0");
	}
}
