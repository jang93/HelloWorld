using UnityEngine;
using System.Collections;

public class FireBallController : MonoBehaviour {

    public float forceMagnitude = 10f;            // The magnitude of force on enemy
    // Use this for initialization
    private Animator animator;
    public GameObject explosion;		// Prefab of explosion effect.
    Vector2 forceDirection;
    void Start () {
        Destroy(gameObject, 2);
    }
	
	// Update is called once per frame
	void Update () {
        

    }
    void OnExplode()
    {
        // Create a quaternion with a random rotation in the z-axis.
        Quaternion randomRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        // Instantiate the explosion where the rocket is with the random rotation.
        Instantiate(explosion, transform.position, randomRotation);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        //all projectile colliding game objects should be tagged "Enemy" or whatever in inspector but that tag must be reflected in the below if conditional
        if (col.gameObject.tag == "Enemy")
        {
            Vector2 charpos = col.gameObject.transform.position;
            Vector2 ballpos = this.transform.position;
            forceDirection = charpos - ballpos;
            Rigidbody2D rigid =col.gameObject.GetComponent<Rigidbody2D>();
            rigid.AddForce(forceDirection*forceMagnitude,ForceMode2D.Impulse);
            //add an explosion or something
            OnExplode();
            //destroy the projectile that just caused the trigger collision
            Destroy(gameObject);
        }   
    }
    }
