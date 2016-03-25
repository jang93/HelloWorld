using UnityEngine;
using System.Collections;
using CnControls;

public class PlayerMobility : MonoBehaviour
{

    public float speed;

    void FixedUpdate()
    {


        //Movement for up down left right
        float input = CnInputManager.GetAxis("Vertical");


        
        GetComponent<Rigidbody2D>().AddForce(gameObject.transform.up * speed * input);


        //For Direction 

        //mouse position needs to be replaced by a joystick controller
        //Only Forwards will remain (no backwards), but direction will be dependent on joystick
         

        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Quaternion rot = Quaternion.LookRotation(transform.position - mousePosition, Vector3.forward);


        transform.rotation = rot;
        transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);


        //Spare Code





    }
}
