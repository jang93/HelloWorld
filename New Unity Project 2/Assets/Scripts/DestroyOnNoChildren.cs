using UnityEngine;
using System.Collections;

//! A small class that destroys a GameObject when it no longer has children.  Useful for effects objects that expire, fade, etc.
public class DestroyOnNoChildren : MonoBehaviour
{
    //! Update function, where the check is performed.
    void Update()
    {
        // does this transform have no children?
        if (transform.childCount == 0)
        {
            // destroy this GameObject
            Destroy(gameObject);
        }
    }
}
