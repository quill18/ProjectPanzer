using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class GameManager : NetworkBehaviour {

	// Use this for initialization
    void Start () {
        // NOTE: Start() runs even before anyone connects to any server
        //StartNewMatch();

    }

    [SyncVar]
    float _TimeLeft = 0.5f;
    public float TimeLeft
    {
        get { return _TimeLeft; }
        set { _TimeLeft = value; }
    }

    public enum TURNSTATE { MOVE, AIM, RESOLVE };

    [SyncVar]
    TURNSTATE _TurnState;
    public TURNSTATE TurnState
    {
        get { return _TurnState; }
        protected set { _TurnState = value; }
    }

    public int TurnNumber { get; protected set; }

    [SyncVar, System.NonSerialized]
    bool matchHasStarted = false;

	// Update is called once per frame
	void Update () {
		
        if(isServer == false)
        {
            return;
        }

        TimeLeft -= Time.deltaTime;

        if ( matchHasStarted == false )
        {
            if (TimeLeft > 0)
            {
                return;
            }
            else
            {
                // It's time to start the match!
                StartNewMatch();
            }
        }


        if(
            (TurnState != TURNSTATE.RESOLVE && (TimeLeft <= 0 || IsPhaseLocked() ) )
            /* TurnState == TURNSTATE.RESOLVE && ResolvePhaseIsCompleted() */ )
        {
            // Advance the turn phase.
            AdvanceTurnPhase();
        }

	}

    public bool TankCanMove(Tank tank)
    {
        return matchHasStarted==true && TurnState == TURNSTATE.MOVE;
    }

    public bool TankCanAim(Tank tank)
    {
        return matchHasStarted == true && TurnState == TURNSTATE.AIM;
    }

    void StartNewMatch()
    {
        matchHasStarted = true;
        TurnNumber = 0;

        // TODO: Show new match title screen, and only when complete, start new turn

        StartNewTurn();
    }

    void StartNewTurn()
    {
        TurnNumber++;
        TurnState = TURNSTATE.MOVE;
        TimeLeft = 10;
        Debug.Log("Starting Turn: " + TurnNumber);
    }

    void AdvanceTurnPhase()
    {
        // NOTE: We could do various delegate functions to avoid having to do switch checks here.

        switch( TurnState )
        {
            case TURNSTATE.MOVE:
                // Move phase is done...what does that mean?
                TurnState = TURNSTATE.AIM;
                TimeLeft = 10;
                break;

            case TURNSTATE.AIM:
                TurnState = TURNSTATE.RESOLVE;
                TimeLeft = 0;
                break;

            case TURNSTATE.RESOLVE:
                Debug.LogError("TODO: Add turn resolution code. (i.e. fire bullets)");
                break;

            default:
                Debug.LogError("THIS SHOULD NEVER HAPPEN.  Unknown TurnState.");
                break;
        }

        Debug.Log("New Phase Started: " + TurnState.ToString());

        // Let's tell all of the tanks that a new phase has started

        Tank[] tanks = GameObject.FindObjectsOfType<Tank>();
        foreach(Tank tank in tanks)
        {
            tank.RpcNewPhase();
        }

    }

    bool IsPhaseLocked()
    {
        // Check to see if all tanks have locked in their phase moves.

        // TODO: Consider having the tank class use a static list to register/unregister
        // live tanks to optimize this step

        Tank[] tanks = GameObject.FindObjectsOfType<Tank>();

        if(tanks == null || tanks.Length == 0)
        {
            Debug.Log("No tanks yet?");
            return false;
        }

        foreach (Tank tank in tanks)
        {
            if( tank.IsLockedIn == false )
            {
                return false;
            }
        }

        return true;
    }

}
