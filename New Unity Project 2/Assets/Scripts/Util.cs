using UnityEngine;
using System.Collections;

//! A static class for various useless utility functions.
public static class Util
{
    //! GetTransform() function.  Returns the Scene heirarchy path to the passed Transform
    //! @param Transform t  The Transform to get the path to.
    //! @return string  The path to the passed Transform.
    public static string GetTransformPath(Transform t)
    {
        // null safety check
        if (!t)
        {
            // return empty string
            return "";
        }

        // initial name is the name of the "tail" or bottom Transform
        string path = t.name;

        // get the current Transform's parent
        Transform c_parent = t.parent;

        // while there is a current parent...
        while (c_parent)
        {
            // prepend the parent's name and a slash to the front of the path
            path = path.Insert(0, c_parent.name + "/");
            // next parent
            c_parent = c_parent.parent;
        }

        // return the built path name
        return path;
    }

    //! GetChildByName() function.  Returns the child Transform specified by name.
    //! @param Transform t  The Transform on which to find the specified child.
    //! @param string child  The name of the child Transform to find.
    //! @param string child  If true, the child name need only match the beginning of the string, otherwise it must be an exact match.
    //! @return Transform  Returns the child, null if it is not found.
    public static Transform GetChildByName(Transform t, string child, bool startsWith)
    {
        // null safety check
        if (!t || string.IsNullOrEmpty(child))
        {
            return null;
        }

        // step through all the Transform's children
        foreach (Transform c in t)
        {
            // just matching on the beginning of the name?
            if (startsWith)
            {
                // does this child name start with the passed name
                if (c.name.StartsWith(child))
                {
                    // return match
                    return c;
                }
            }

            // exact name check
            if (c.name == child)
            {
                // return match
                return c;
            }
        }

        // no child returned above, so it wasn't found, return null
        return null;
    }

    //! LayerMaskToLayer() function.  Converts a layer mask to a layer integer.
    //! @param LayerMask mask  The layer mask to convert.
    //! @return int  The layer mask's integer value.
    public static int LayerMaskToLayer(LayerMask mask)
    {
        // go through all "legal" masks (in a 32 bit value range)
        for (int i = 0; i < 32; i++)
        {
            // bit shift the value, and check it against the passed mask
            if ((1 << i) == mask.value)
            {
                // this mask matches, so return the integer value
                return i;
            }
        }

        // no mask match, so return -1
        return -1;
    }
}
