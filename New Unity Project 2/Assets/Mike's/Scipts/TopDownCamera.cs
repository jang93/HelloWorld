using UnityEngine;
using System.Collections;

//! A simple top-down camera controller that tracks the specified object
public class TopDownCamera : MonoBehaviour
{
    //! The GameObject to follow.
    public GameObject followObject;
    //! The GameObject where the AudioListener is (will set to follow object position)
    public Transform audioListener;

    //! The tracking offset, which is initialized to the offset from the followed object.
    Vector3 followOffset = new Vector3(0, 0, 0);

    //! Start() function.
    void Start()
    {
        // is a follow object assigned?
        if (followObject)
        {
            // init follow offset
            followOffset = transform.position - followObject.transform.position;

            // audio listener assigned?
            if (audioListener)
            {
                // put the audio listener object at the follow object position
                audioListener.position = followObject.transform.position;
            }
        }
        else
        {
            // log warning
            Debug.LogWarning("TopDownCamera.Start() " + name + " has no follow object assigned!");
        }
    }

    //! Update() function.
    void Update()
    {
        // follow object assigned?
        if (followObject)
        {
            // update position
            transform.position = new Vector3(followObject.transform.position.x + followOffset.x, transform.position.y, followObject.transform.position.z + followOffset.z);
        }
    }
}
