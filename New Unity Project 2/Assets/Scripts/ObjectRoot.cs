using UnityEngine;
using System.Collections;

//! A small class that moves the GameObject to the named root GameObject (creating it if necessary).
//! Very handy for putting Units under a "Units" root, effects under "Effects", etc, to keep the Scene heirarchy neat during run-time.
public class ObjectRoot : MonoBehaviour
{
    //! The name of the root GameObject to put this GameObject under.
    public string rootName;

    //! Start function.
    void Start()
    {
        // has a root name been specified?
        if (!string.IsNullOrEmpty(rootName))
        {
            // try to find the root GameObject
            GameObject root = GameObject.Find(rootName);

            // if it doesn't exist, create it
            if (!root)
            {
                root = new GameObject(rootName);
            }

            // assign this GameObject's parent transform
            transform.parent = root.transform;
        }
    }

}
