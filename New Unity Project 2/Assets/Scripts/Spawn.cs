using UnityEngine;
using System.Collections;

//! Spawn class.
public class Spawn : MonoBehaviour
{
    //! Enable to log debug messages.
    public bool debug;
    //! If flagg, the debug flag on spawned Units will be enabled.
    public bool debugUnit;

    //! Object(s) to spawn.  If there is more than one, one is chosen randomly each spawn.
    public Transform[] spawnObjects;
    //! ArrayList to store current active spawned GameObjects.
    protected ArrayList currentObjects = new ArrayList();

    //! Number of objects to spawn. -1 for infinite.
    public int spawnCount = -1;
    //! Internal counter for number of objects to spawn.
    int numToSpawn;

    //! Initial spawn delay.
    public float initialDelay = 1f;
    //! Delay between spawns
    public float spawnDelay = 1f;
    //! Internal counter for next spawn.
    float spawnWait;

    //! If enabled, objects will only be spawn off screen (i.e. out of view).
    public bool spawnOnlyOffScreen = true;
    //! Internal flag to indicate if the spawn is visible.
    bool isVisible;

    //! If enabled, objects will be spawned facing the player.  Otherwise, with the spawn's orientation.
    public bool spawnFacingPlayer;

    //! Internal reference to the Player GameObject
    GameObject player;

    //! The GameObject are set active when an object is spawned.  Useful for spawn effects.
    public GameObject[] activateOnSpawn;

    //! Path node assigned to Units when they are spawned.
    public GameObject pathNode;
    //! Path mode assigned to Units when they are spawned.
    public UnitAI.ePathMode pathMode = UnitAI.ePathMode.Once;
    //! Path dir assigned to Units when they are spawned.
    public int pathDir = 1;

    //! Tether point assigned to Units when they are spawned.
    public GameObject tetherPoint;
    //! Tether distance assigned to Units when they are spawned.
    public float tetherDistance = 5f;

    //! Start() function.
    void Start()
    {
        // check that spawn object(s) assigned in Inspector
        if (spawnObjects.Length == 0)
        {
            // log error
            Debug.LogError("Spawn.Start() " + name + " has no spawnObjects assigned!");
            // disable this Component
            enabled = false;
        }

        // find the player GameObject
        player = GameObject.FindGameObjectWithTag("Player");
    }

    //! OnEnable() function.  Handles the Spawn being re-enabled.
    void OnEnable()
    {
        // reset spawn wait timer
        spawnWait = initialDelay;
        // reset number to spawn
        numToSpawn = 0;
    }

    //! Update() function.  Virtual so subclasses can override.
    public virtual void Update()
    {
        // update current objects
        for (int i = 0; i < currentObjects.Count; )
        {
            // if the current object has been destroyed, remove it from the current objects
            if (currentObjects[i] == null)
            {
                // remove this object
                currentObjects.RemoveAt(i);
            }
            else
            {
                // next object
                ++i;
            }
        }

        // check to see if another object should be spawned
        // spawn count of -1 for infinite spawns, or number spawned is less than spawn count
        // spawn off screen is false, or the spawn is not on screen
        if ((spawnCount == -1 || numToSpawn < spawnCount) && (!spawnOnlyOffScreen || !IsOnScreen()))
        {
            // decrement spawn wait
            spawnWait -= Time.deltaTime;

            // time to spawn yet?
            if (spawnWait > 0f)
            {
                // not yet
                return;
            }

            // grab the first spawn object from the array
            Transform spawnObject = spawnObjects[0];

            // is there more than one spawn object assigned
            if (spawnObjects.Length > 1)
            {
                // select a random spawn object
                spawnObject = spawnObjects[Random.Range(0, spawnObjects.Length)];
            }

            // create the spawn object
            Transform spawn = (Transform)Instantiate(spawnObject, transform.position, transform.rotation);

            if (debug)
                Debug.Log("Spawn.Update() " + name + " spawned " + spawn.name);

            // count up the spawned object
            ++numToSpawn;

            // add the spawn object to the current objects
            currentObjects.Add(spawn);

            // should the object face the player?
            if (spawnFacingPlayer && player)
            {
                // set the spawned object to face the player
                spawn.rotation = Quaternion.LookRotation((player.transform.position - transform.position).normalized);
            }

            // any objects to activate on spawn?
            foreach (GameObject target in activateOnSpawn)
            {
                // activate object
                target.active = true;
            }

            // any AI properties to assign?
            if (debugUnit || pathNode || tetherPoint)
            {
                // try to get a UnitAI Component
                UnitAI unitAI = (UnitAI)spawn.gameObject.GetComponent("UnitAI");

                // did we get a UnitAI
                if (unitAI)
                {
                    // debug flag?
                    if (debugUnit)
                    {
                        // set the debug flag
                        unitAI.debug = true;
                    }

                    // path node assigned?
                    if (pathNode)
                    {
                        // set the path properties
                        unitAI.pathNode = pathNode;
                        unitAI.pathMode = pathMode;
                        unitAI.pathDir = pathDir;
                    }

                    // tether point assigned?
                    if (tetherPoint)
                    {
                        // set the tether point properties
                        unitAI.tetherPoint = tetherPoint;
                        unitAI.tetherDistance = tetherDistance;
                    }
                }
            }

            // finished spawning?
            if (spawnCount != -1 && numToSpawn == spawnCount)
            {
                // disable this Component
                enabled = false;
            }

            // reset spawn wait timer
            spawnWait = spawnDelay;
        }
    }

    //! IsOnScreen() function.  Checks to see if this Spawn is on screen/visible.
    //! @return bool  Returns true if the Spawn is on screen, false if not.
    bool IsOnScreen()
    {
        // get the screen position of this spawn
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

        // check to see if the spawn position is "on screen"
        if (screenPos.x > 0 && screenPos.y > 0 && screenPos.x < Screen.width && screenPos.y < Screen.height)
        {
            if (debug)
                Debug.Log("Spawn.IsOnScreen() " + name + " true");

            // spawn is on screen
            return true;
        }

        if (debug)
            Debug.Log("Spawn.IsOnScreen() " + name + " false");

        // spawn is off screen
        return false;
    }
}
