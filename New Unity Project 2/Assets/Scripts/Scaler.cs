using UnityEngine;
using System.Collections;

//! A small class that scales a Transform over time.  Useful for many effects (e.g. "growing" or "shrinking").
public class Scaler : MonoBehaviour
{
    //! If enabled, the start scale is copied from the Transform on Start(), overriding startScale.
    //! This is generally the case for objects you want to scale down.
    public bool inheritStartScale;

    //! The starting (initial) scale of the Transform.
    public Vector3 startScale = Vector3.zero;
    //! The ending (target) scale of the Transform.
    public Vector3 endScale = new Vector3(1, 1, 1);

    //! The duration of the scale operation (in seconds).
    public float scaleTime = 1f;
    //! Internal variable to track the elapsed time of the scale operation (in seconds).
    float scaleElapsed;

    //! Start() function.
    void Start()
    {
        // inherit starting scale from the GameObject?
        if (inheritStartScale)
        {
            // set starting scale
            startScale = transform.localScale;
        }
    }

    //! Update() function.
    void Update()
    {
        // update scale time
        scaleElapsed += Time.deltaTime;

        // still scaling?
        if (scaleElapsed < scaleTime)
        {
            // set the local scale by lerping between start scale and end scale
            transform.localScale = Vector3.Lerp(startScale, endScale, scaleElapsed / scaleTime);
        }
        else
        {
            // finished scaling, so set scale to end scale
            transform.localScale = endScale;
            // disable this Component
            enabled = false;
        }
    }
}
