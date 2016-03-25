using UnityEngine;
using System.Collections;

//! A class to draw Gizmos in the Editor (for spawns, etc)
public class Gizmo : MonoBehaviour
{
    //! The name of the texture to use on the Gizom.  Textures should be located in Assets/Gizoms.
    public string gizmo;

    //! OnDrawGizmos function
    void OnDrawGizmos()
    {
        // is a gizmo string entered in the Inspector?
        if (!string.IsNullOrEmpty(gizmo))
        {
            // draw the specified gizmo
            Gizmos.DrawIcon(transform.position, gizmo);
        }
    }
}
