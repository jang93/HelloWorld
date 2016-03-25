using UnityEngine;
using System.Collections;

//require Character Script to be attached to this object.
[RequireComponent(typeof(ZombieScript))]
public class ZombieInput : MonoBehaviour
{
    //private reference to the character script for making calls to the public api.
    private ZombieScript character;

    //reference to the camera
    private Camera mainCamera;

    private Vector2 heading;

    /// <summary>
    /// Use this function for initialization of just this component.
    /// </summary>
    private void Awake()
    {
        //nothing special to initialize here.
        heading = Vector2.zero;
    }

    /// <summary>
    /// Use this function for initialization that depends on other components being created.
    /// </summary>
    private void Start()
    {
        //we require a built up version of the character script.
        this.character = this.GetComponent<ZombieScript>();

        this.mainCamera = Camera.main;
    }

    /// <summary>
    /// use this function to process updates as fast as the game can process them.
    /// </summary>
    void Update()
    {
        //check for touches
        if (Input.touchCount > 0)
        {
            //what was the position?
            Vector2 touchPosition = Input.GetTouch(0).position;
            Vector3 touchWorldPosition = this.mainCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, 15));
            //where is our character currently?
            Vector3 characterPosition = character.gameObject.transform.position;
            //vector math says point to get to - current position = heading.
            this.heading = new Vector2(touchWorldPosition.x - characterPosition.x, touchWorldPosition.y - characterPosition.y);
            //make sure we don't surpass 1.
            this.heading.Normalize();
        }

    }

    /// <summary>
    /// use this function to process updates that should be synchronized 
    /// with the physics engine.  Good for continuous input functions for movement.
    /// </summary>
    void FixedUpdate()
    {
        //get the x factor of movement.
        float xMovement = Input.GetAxis("Horizontal");
        //get the y factor of movement.
        float yMovement = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(xMovement, yMovement);

        if (movement.magnitude >= 0)
        {
            this.heading = movement.normalized;
        }

        //use our character script reference to make a call into its public api
        //to move the character by our input factor.
        if (!Spells.attacking)
        {
            character.Move(heading);
        }

        
    }
}