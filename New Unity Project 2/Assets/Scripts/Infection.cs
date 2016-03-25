using UnityEngine;
using System.Collections;

//! This class controls zombie infection in the Simulation game mode.  It is added to a Unit GameObject when hit by a zombie melee weapon.
public class Infection : MonoBehaviour
{
    //! The Unit this infection is attached to.
    Unit unit;

    //! Infection rate (per second).
    public float infectionSpeed = 4f;
    //! Internal variable tracking current infection total.
    float infection;

    //! The minimum delay between death and rise as a zombie.
    public float minRiseTime = 5f;
    //! The maximum delay between death and rise as a zombie.
    public float maxRiseTime = 10f;
    //! Internal variable tracking time to rise as a zombie.
    float riseTime;

    //! Start function.
    void Start()
    {
        // get the Unit Component on this GameObject
        unit = (Unit)GetComponent("Unit");

        // verify Unit
        if (!unit)
        {
            // log warning
            Debug.LogWarning("Infection.Start() " + name + " could not find Unit!");
            // disable this Component 
            enabled = false;
            // and return
            return;
        }

        // calc randomized rise time
        riseTime = Random.Range(minRiseTime, maxRiseTime);
    }

    //! Update function.
    void Update()
    {
        // is the Unit still alive
        if (!unit.Dead)
        {
            // increment infection
            infection += infectionSpeed * Time.deltaTime;

            // is the infection greater than the Unit's current health?
            if (infection > unit.Health)
            {
                // kill the Unit
                unit.Die();
            }
        }
        else
        {
            // decrement rise time
            riseTime -= Time.deltaTime;

            // time to rise?
            if (riseTime <= 0f)
            {
                // get the sim manager
                SimulationManager simMgr = GameManager.Instance as SimulationManager;

                // null safety check
                if (!simMgr)
                {
                    // log warning
                    Debug.LogWarning("Infection.Update() " + name + " could not get SimulationManager!");
                }
                else
                {
                    // create a zombie based on the prefab specified in the sim manager
                    Instantiate(simMgr.zombiePrefab, new Vector3(transform.position.x, 1, transform.position.z), Quaternion.LookRotation(transform.up));
                }

                // destroy this GameObject
                Destroy(gameObject);
            }
        }
    }

    //! Property to return current infection.
    public float CurrentInfection
    {
        get { return infection; }
    }
}
