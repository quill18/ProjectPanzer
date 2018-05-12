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

    bool haveFiredBullets = false;
    bool bulletsHaveSpawned = false;
    List<GameObject> activeResolutionsObjects;

    Queue<GameObject> eventQueue;
    GameObject currentEvent;

    public GameObject NewTurnAnimationPrefab;

    // Update is called once per frame
    void Update () {
        if(isServer == false)
        {
            return;
        }

        // Process any events that are queued up, pausing game logic while that's happening
        if( ProcessEvent() )
        {
            // We are processing an event, so cut the update short.
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


        if( TurnState == TURNSTATE.RESOLVE )
        {
            // We are in the RESOLVE phase, so process it

            if( ProcessResolvePhase() == false )
            {
                // Resolve phase is over, so let's start a new turn
                AdvanceTurnPhase();
            }

        }
        else
        {
            // We are in MOVE or AIM
            if (TimeLeft <= 0 || IsPhaseLocked())
            {
                AdvanceTurnPhase();
            }
        }


	}

    static public GameManager Instance()
    {
        // TODO: Cache Me
        return GameObject.FindObjectOfType<GameManager>();
    }

    public void EnqueueEvent( GameObject go )
    {
        if(eventQueue == null)
        {
            eventQueue = new Queue<GameObject>();
        }

        go.SetActive(false);    // Not allowed to be active while it's in the queue
        eventQueue.Enqueue(go);
    }

    public bool IsProcessingEvent()
    {
        if (currentEvent == null)
        {
            // Nothing in the queue
            return false;
        }

        return true;
    }

    bool ProcessEvent()
    {
        if(currentEvent != null)
        {
            // Event is still running, do nothing.
            return true;
        }

        if(eventQueue == null || eventQueue.Count == 0)
        {
            // Nothing in the queue
            return false;
        }

        currentEvent = eventQueue.Dequeue();
        currentEvent.SetActive(true);

        return true;
    }

    public void RegisterResolutionObject( GameObject o )
    {
        activeResolutionsObjects.Add(o);
    }

    public void UnregisterResolutionObject( GameObject o )
    {
        activeResolutionsObjects.Remove(o);
    }

    bool ProcessResolvePhase()
    {
        Tank[] tanks = GetAllTanks();

        // TODO:  Add a step that has a little animation entering this phase,
        // which lasts at least a fraction of a second just to make sure that
        // we have received the final aiming instructions from a lagged client.

        if(haveFiredBullets == false)
        {
            activeResolutionsObjects = new List<GameObject>();
            bulletsHaveSpawned = false;

            foreach (Tank tank in tanks)
            {

                tank.Fire();
            }

            haveFiredBullets = true;
        }

        if(activeResolutionsObjects.Count > 0)
        {
            bulletsHaveSpawned = true;
        }

        Debug.Log(activeResolutionsObjects.Count);

        // Are any bullets/explosions still on screen?
        if(bulletsHaveSpawned && activeResolutionsObjects.Count == 0 )
        {
            Debug.Log("Returning false!");
            // No more bullets/explosions/etc...  Resolution phase is over!
            return false;
        }

        return true; // We still have more to do

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
        haveFiredBullets = false;
        Debug.Log("Starting Turn: " + TurnNumber);

        GameObject ntgo = Instantiate(NewTurnAnimationPrefab);
        Debug.Log(ntgo);
        EnqueueEvent(ntgo);
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
                StartNewTurn();
                break;

            default:
                Debug.LogError("THIS SHOULD NEVER HAPPEN.  Unknown TurnState.");
                break;
        }

        Debug.Log("New Phase Started: " + TurnState.ToString());

        // Let's tell all of the tanks that a new phase has started

        Tank[] tanks = GetAllTanks();
        foreach (Tank tank in tanks)
        {
            tank.RpcNewPhase();
        }

    }

    Tank[] GetAllTanks()
    {
        // TODO: Consider having the tank class use a static list to register/unregister
        // live tanks to optimize this step

        return GameObject.FindObjectsOfType<Tank>();
    }

    bool IsPhaseLocked()
    {
        // Check to see if all tanks have locked in their phase moves.

        Tank[] tanks = GetAllTanks();

        if (tanks == null || tanks.Length == 0)
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
