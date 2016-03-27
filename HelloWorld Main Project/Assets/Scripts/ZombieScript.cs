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

	public SpriteRenderer healthBar; 
	public int maxHealth = 100;

	private int health = 100;
	private Vector2 healthScale;
	public Action deathFunction = () => {};

    void Awake()
    {
    }

    private void Start()
    {
        //cached animator

        this.animator = this.GetComponent<Animator>();
        this.cachedRigidBody2D = this.GetComponent<Rigidbody2D>();
		healthScale=healthBar.transform.localScale;

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


	public void AdjustHealth(int amount) 
	{
		//adjust current health by amount.
		this.health+=amount;

		//make sure character can't surpass max health
		if(this.health > this.maxHealth)
		{
			this.health = maxHealth;
		}
		else if(this.health <= 0)
		{
			//execute the death function 
			this.deathFunction();
		}

		//update the health bar with new amount
		this.UpdateHealthBar();
	}


	public int GetHealth()
	{
		return this.health;
	}


	private void UpdateHealthBar()
	{
		// Set the health bar color between Red and Green based on current health.
		healthBar.material.color = Color.Lerp(Color.green, Color.red, 1 - this.GetHealth() * 0.01f);

		// Set the scale of the health bar to be proportional to the player's health.
		healthBar.transform.localScale = new Vector3(healthScale.x * this.GetHealth() * 0.01f, 1, 1);
	}

}







