using UnityEngine;
 
using System.Collections;

using System;

public class ZombieScript : MonoBehaviour

{
    //max speed of character
    public float maxSpeed = 5.0f;
    //adjustment for rotation based on sprite starting orientation.
    public float facingAngleAdjustment = 0.0f;
    private Animator animator;
    //cached version of our physics rigid body.
    private Rigidbody2D cachedRigidBody2D;

    void Awake()
    {
    }

    private void Start()
    {
        //cached animator

        this.animator = this.GetComponent<Animator>();
        this.cachedRigidBody2D = this.GetComponent<Rigidbody2D>();
    }

    public void Move(Vector2 movement)
    {
        //move the rigid body, which is part of the physics system
        //This ensures smooth movement.
        this.cachedRigidBody2D.velocity = new Vector2(movement.x * maxSpeed, movement.y * maxSpeed);
        //take the absolute value and add, because x or y 
        //may be negative and potentially cancel eachother out.
        float speed = Mathf.Abs(movement.x) + Mathf.Abs(movement.y);

        //set the speed variable in the animation component to ensure proper state.
        this.animator.SetFloat("Speed", speed);
        //convert the vector into a radian angle, 
        //convert to degrees and then adjust for the 
        //spider's starting orientation
        float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg + facingAngleAdjustment;

        //don't rotate if we don't need to.
        if (speed > 0.0f)
        {
            //rotate by angle around the z axis.
            this.transform.rotation = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1));
        }


    }

}