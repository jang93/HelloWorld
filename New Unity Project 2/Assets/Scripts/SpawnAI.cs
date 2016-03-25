using UnityEngine;
using System.Collections;

//! A Spawn subclass to perform certain AI-specific operations.
public class SpawnAI : Spawn 
{
    //! Time until next dead check (once per second).
    float nextDeadCheck;

    //! Update() function override.  Tracks AIs and removes them from the currentObjects array when they are dead (as dead AIs are set inactive, not destroyed).
	public override void Update () 
    {
        // this is a little expensive, so only do it once a second
        nextDeadCheck -= Time.deltaTime;

        // time to check?
        if (nextDeadCheck <= 0f)
        {
            // go through object in the current objects array
            for (int i = 0; i < currentObjects.Count; )
            {
                // get the Transform
                Transform t = (Transform)currentObjects[i];

                // null safety check
                if (t)
                {
                    // try to get a UnitAI
                    UnitAI unitAI = (UnitAI)t.GetComponent("UnitAI");

                    // got UnitAI, unit is dead
                    if (unitAI && unitAI.Dead)
                    {
                        if (debug)
                            Debug.Log("SpawnAI.Update() " + name + " removing dead UnitAI " + unitAI.name);

                        // remove dead unit from the current objects
                        currentObjects.RemoveAt(i);
                    }
                    else
                    {
                        // next object
                        ++i;
                    }
                }
                else
                {
                    // next object
                    ++i;
                }
            }

            // reset next check timer
            nextDeadCheck = 1f;
        }

        // call base Update()
        base.Update();
    }

}
