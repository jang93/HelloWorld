using UnityEngine;
using System.Collections;

//! Base class for Units.  A Unit is a moveable, destructible GameObject which may be controlled by the player (UnitPlayer subclass) or AI (UnitAI subclass).
//! This base class is generally responsible for applying movement commands, movement speed, handling pickups, damage effects, sound effects, etc.
public class Unit : Damageable
{
    //! The Unit's current move command as set by subclasses in Update()
    protected Vector3 move = Vector3.zero;

    //! Unit walk speed (m/s).
    public float walkSpeed = 1f;
    //! Unit run speed (m/s).
    public float runSpeed = 1f;
    //! Unit turns speed (angles/s).
    public float turnSpeed = 180f;
    //! A random speed scale (0-1) to apply to Units (to create a little variation)
    public float randomSpeedScalar = 0f;
    //! Internal flag to indicate if the Unit is currently running.
    protected bool running;

    //! Weapons attached to the Unit
    //! NOTE there is a "nod" towards supporting multiple weapons, but at the moment only one weapon is used by Units.
    protected ArrayList weapons = new ArrayList();

    //! Mask for enemies of this Unit.
    public LayerMask enemies;
    //! Mask for friendlies of this Unit.
    public LayerMask friendlies;

    //! Current pickup object available to this Unit.
    protected GameObject pickup;
    //! Range to check for pickup objects
    //public float 

    //! The Unit's current target move position
    protected Vector3 moveToPosition;
    //! A flag to indicate that this Unit has a move to position
    protected bool hasMoveToPosition;

    //! Start() function.
    public override void Start()
    {
        // call base Start()
        base.Start();

        // random speed scalar assigned?
        if (randomSpeedScalar != 0f)
        {
            // get a random speed scalar of 1 +/- speed scalar
            float scalar = 1f + (Random.Range(-randomSpeedScalar, randomSpeedScalar));
            // apply to walk, run and turn speeds
            walkSpeed *= scalar;
            runSpeed *= scalar;
            turnSpeed *= scalar;
        }

        // get attached weapons
        Weapon[] attached = GetComponentsInChildren<Weapon>();

        // "pick up" attached weapons
        foreach (Weapon weapon in attached)
        {
            Pickup(weapon.gameObject);
        }
    }

    //! Update() function.
    public virtual void Update()
    {
        // if this Unit has a rigidbody, movement is done in FixedUpdate()
        if (GetComponent<Rigidbody>())
        {
            return;
        }

        // move assigned?
        if (move != Vector3.zero)
        {
            // calc look angle (the angle difference between forward and movement vector)
            float lookAngle = Vector3.Angle(transform.forward, move);
            
            // apply move to position
            // speed is scaled by facing, so units move slower when moving backwards
            transform.position += move * (running ? runSpeed : walkSpeed) * Time.deltaTime / Mathf.Clamp(1.5f - (lookAngle / 180f), 0.5f, 1f);
        }
    }

    //! FixedUpdate() function.
    public virtual void FixedUpdate()
    {
        // if this Unit doesn't have a rigidbody, move is done in Update()
        if (!GetComponent<Rigidbody>())
        {
            return;
        }

        // move assigned?
        if (move != Vector3.zero)
        {
            // calc move speed
            Vector3 moveDistance = move * (running ? runSpeed : walkSpeed);

            // calc look angle (the angle difference between forward and movement vector)
            float lookAngle = Vector3.Angle(transform.forward, move);

            if (lookAngle > 0f)
            {
                // speed is scaled by facing, so units move slower when moving backwards
                moveDistance *= Mathf.Clamp(1.5f - (lookAngle / 180f), 0.5f, 1f);
            }

            // apply movement to rigidbody velocity
            GetComponent<Rigidbody>().velocity = new Vector3(moveDistance.x, GetComponent<Rigidbody>().velocity.y, moveDistance.z);

            if (debug)
                Debug.Log("Unit.FixedUpdate() speed " + GetComponent<Rigidbody>().velocity.magnitude);
        }
    }

    //! Die() function.
    public override void Die()
    {
        // call base.Die()
        base.Die();

        // drop any Item (carried) weapons this Unit is carrying
        for (int i = 0; i < weapons.Count; )
        {
            // get the weapon
            Weapon weapon = (Weapon)weapons[i] as Weapon;

            // null safety check
            if (weapon)
            {
                // disable the weapon Component
                weapon.enabled = false;

                // if it is not an Unarmed weapon
                if (weapon.weaponType != Weapon.WeaponType.Unarmed)
                {
                    // drop it
                    Drop(weapons[i] as Weapon);
                }
                else
                {
                    // next weapon
                    ++i;
                }
            }
        }
    }

    //! Pickup() function.
    //! @param GameObject item  The item to pick up.
    protected void Pickup(GameObject item)
    {
        // null safety check
        if (!item)
        {
            return;
        }

        // does the item have a rigodbody?
        if (item.GetComponent<Rigidbody>())
        {
            // set it to kinematic with no gravity
            item.GetComponent<Rigidbody>().isKinematic = true;
            item.GetComponent<Rigidbody>().useGravity = false;
        }

        // if the item has a collider, set it to be a trigger, which is an easy/quick way to "disable" collisions
        if (item.GetComponent<Collider>())
        {
            item.GetComponent<Collider>().isTrigger = true;
        }

        // set the item to not be hit by ray casts
        item.layer = LayerMask.NameToLayer("Ignore Raycast");

        if (debug)
            Debug.Log ("Unit.Pickup() " + name + " picked up " + item.name + " set layer to " + LayerMask.LayerToName(item.layer));

        // find the weapon attach point (i.e. hand)
        Transform hand = transform.Find("Weapon");

        // if there is a hand, attach the item to it, otherwise attach it to this transform
        item.transform.parent = hand ? hand : transform;

        // zero the item's position/rotation
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        // get a Weapon Component from the item
        // could just cast the item, but this way is a little more general purpose
        Weapon weapon = (Weapon)item.GetComponent("Weapon");

        // got a weapon?
        if (weapon)
        {
            // add the weapon to the Unit's weapons
            weapons.Add(weapon);
            // set the weapon's Unit reference
            weapon.Unit = this;
            // enable the weapon
            weapon.enabled = true;
        }
    }

    //! Drop() function.
    //! @param Weapon weapon  The weapon to drop.
    protected void Drop(Weapon weapon)
    {
        // nul safety check
        if (!weapon)
            return;

        // remove the weapon from the Unit's weapons
        weapons.Remove(weapon);

        // is it some kind of strange weapon with no rigidbody and collider?
        if (!weapon.GetComponent<Rigidbody>() && !weapon.GetComponent<Collider>())
        {
            // just destroy it
            Destroy(weapon.gameObject);
            return;
        }

        // does the weapon have a rigidbody?
        if (weapon.GetComponent<Rigidbody>())
        {
            // set it to non-kinematic with gravity
            weapon.GetComponent<Rigidbody>().isKinematic = false;
            weapon.GetComponent<Rigidbody>().useGravity = true;
        }

        // does the weapon have a collider?
        if (weapon.GetComponent<Collider>())
        {
            // make it not a trigger, "enabling" normal collisions
            weapon.GetComponent<Collider>().isTrigger = false;
        }

        // set the weapon layer to Item
        weapon.gameObject.layer = LayerMask.NameToLayer("Item");

        // detach the weapon from this transform
        weapon.transform.parent = null;

        // turn off input on the weapon
        weapon.Input = false;

        // null the weapon's Unit reference
        weapon.Unit = null;

        // disable the weapon Component
        weapon.enabled = false;
    }

    //! OnCollisionEnter() callback function.
    //! @param Collision collision  The collision info.
    void OnCollisionEnter(Collision collision) 
    {
        if (debug)
            Debug.Log("Unit.OnCollisionEnter() " + name + " hit " + collision.collider.name);

        // has this Unit collided with an Item?
        if (collision.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            // if the Unit is unarmed, pick up the weapon
            if (weapons.Count == 0)
            {
                Pickup(collision.gameObject);
            }
            else
            {
                // assign the pickup reference (for the player)
                pickup = collision.gameObject;
            }
        }
    }

    //! OnCollisionExit() callback function.
    //! @param Collision collision  The collision info.
    void OnCollisionExit(Collision collision)
    {
        if (debug)
            Debug.Log("Unit.OnCollisionEnter() " + name + " hit " + collision.collider.name);

        // has this Unit collision with an Item ended?
        if (collision.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            // is this Item the current pickup reference?
            if (pickup == collision.gameObject)
            {
                // null pickup reference (for the player)
                pickup = null;
            }
        }
    }

    //! Property to access weapons ArrayList
    public ArrayList Weapons
    {
        get { return weapons; }
    }

    //! Property to access moveToPosition
    public Vector3 MoveTo
    {
        get 
        { 
            return moveToPosition; 
        }
        set 
        { 
            // set the move to position
            moveToPosition = value;
            // set the "has move to position" flag
            hasMoveToPosition = true;
        }
    }

}
    