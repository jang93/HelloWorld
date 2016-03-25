using UnityEngine;
using System.Collections;

//! A simple scrolling controllers that moves the Transform along X & Z by keyboard input.
public class ScrollingCamera : MonoBehaviour
{
    //! The scroll speed (m/s)
    public float moveSpeed = 1f;

    //! Update() function.
    void Update()
    {
        transform.position += Vector3.right * Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime;
        transform.position += Vector3.forward * Input.GetAxisRaw("Vertical") * moveSpeed * Time.deltaTime;
    }
}
