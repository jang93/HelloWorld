using UnityEngine;
using System.Collections;

//!  This class randomizes the size of Building cubes in the Simulation game.
public class Building : MonoBehaviour
{
    //!  Minimum size of the Building.
    public Vector3 minSize = new Vector3(4, 1, 4);

    //!  Maximum size of the Bulding.
    public Vector3 maxSize = new Vector3(8, 10, 8);

    //!  Applies scale value to the Building's size.  For example, we want each story to be 3 meters.
    public Vector3 sizeScalar = new Vector3(1, 3, 1);

    //!  Start funcition.
    void Start()
    {
        // A random scale between minSize/maxSize is chosen (casting to int ensures round numbers, no fractions)
        // then the Building is scaled by sizeScalar
        transform.localScale = new Vector3(Random.Range((int)minSize.x, (int)maxSize.x + 1) * sizeScalar.x, Random.Range((int)minSize.y, (int)maxSize.y + 1) * sizeScalar.y, Random.Range((int)minSize.z, (int)maxSize.z + 1) * sizeScalar.z);

        // Adjust the building's y position to accomodate the new height
        transform.position = new Vector3(transform.position.x, transform.localScale.y * 0.5f, transform.position.z);
    }

}
