using UnityEngine;
using System.Collections;

//! A simple class that toggles the active state of target GameObjects when enabled.
public class Toggle : MonoBehaviour
{
    //! GameObjects to toggle.
    public GameObject[] targets;

    //! OnEnable() function.
    void OnEnable()
    {
        // no objects assigned?
        if (targets.Length == 0)
            return;

        // only one object?
        if (targets.Length == 1)
        {
            // toggle target object active state
            targets[0].SetActiveRecursively(!targets[0].active);
        }
        else
        {
            // get a random index
            int i = Random.Range(0, targets.Length);
            // toggle the random object active state
            targets[i].SetActiveRecursively(!targets[i].active);
        }
    }

}
