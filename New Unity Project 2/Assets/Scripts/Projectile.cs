using UnityEngine;
using System.Collections;

//! This class is placed on projectiles (as from weapons) and performs both collision callback handling (OnCollisionEnter) and tunneling prevention 
//! (for very fast-moving projectiles) by casting a ray in front of the moving projectile.
public class Projectile : MonoBehaviour
{
    //! Enabled this to log debug messages.
    public bool debug;

    //! The amount of damage done by the projectile.
    public float damage;
    //! The velocity of the projectile.
    public float velocity;
    //! The Unit that fired the projectile weapon.
    public Unit unit;
    //! The hit mask used by the projectile collision checks.
    public LayerMask hitMask;

    //! Hit effects created when the projectile hits something (anything).
    public Transform[] hitEffects;
    //! Hit effects created when the projectile hits the world (specifically).
    public Transform[] hitWorldEffects;
    //! Attached transforms that should be removed when the projectile is destroyed (e.g. lingering rocket smoke).
    public Transform[] detachOnDestroy;

    //! FixedUpdate() function.
    void FixedUpdate()
    {
        // on the first FixedUpdate() call, this sets the projectile's velocity
        if (GetComponent<Rigidbody>().velocity == Vector3.zero)
        {
            // set the projectile's velocity by adding a force on the projectile's forward direction
            GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * velocity, ForceMode.VelocityChange);
        }

        // check for a hit
        CheckHit();
    }

    //! CheckHit() function, perform drill-through prevention using a ray cast in front of the moving projectile.
    void CheckHit()
    {
        // don't do this if the projectile is not moving
        if (velocity == 0f)
        {
            if (debug)
                Debug.Log ("Projectile.CheckHit() " + name + " has 0 speed");

            return;
        }

        // a RaycastHit to receive collision info
        RaycastHit hit;

        // check for a collision along the projectile's trajectory (forward velocity)
        // this prevents tunneling (i.e. fast projectiles that "pop through" objects)
        if (Physics.Raycast(transform.position, transform.forward, out hit, velocity * Time.fixedDeltaTime, hitMask | GameManager.Instance.worldMask | GameManager.Instance.groundMask))
        {
            // a collision was detected, so call Hit() function
            Hit(hit.collider, hit.point, hit.normal);
        }
    }

    //! OnCollisionEnter() function.  Handles collision callbacks.
    //! @param Collision hit  The collision info.
    void OnCollisionEnter(Collision hit)
    {
        // a collision was detected, so call Hit() function
        Hit(hit.collider, hit.contacts[0].point, hit.contacts[0].normal);
    }

    //! Hit() function.  Applies damage to the hit object and generates hit effects.
    //! @param Collider hit  The collider that was hit by this projectile.
    //! @param Vector3 position  The position of the hit.
    //! @param Vector3 normal  The normal of the hit.
    void Hit(Collider hit, Vector3 position, Vector3 normal)
    {
        // null safety check
        if (!hit)
            return;

        if (debug)
            Debug.Log("Projectile.Hit() " + name + " hit " + hit.name);

        // create hit effects
        foreach (Transform he in hitEffects)
        {
            Instantiate(he, position, Quaternion.LookRotation(normal));
        }

        // check for a Damageable Component on the hit object
        Damageable damageable = (Damageable)hit.gameObject.GetComponent("Damageable");

        // got Damageable Component?
        if (damageable)
        {
            // damage the hit object
            damageable.Damage(damage, position, normal, unit, true);
        }
        else
        {
            // not a Damageable, so create world hit effects
            foreach (Transform hwe in hitWorldEffects)
            {
                Instantiate(hwe, position, Quaternion.LookRotation(normal));
            }
        }

        // if there are any objects to detach when this projectile is hit, do it now
        // this is useful for "trails" we want to persist after the projectile is destroyed
        // (e.g. rocket smoke, etc)
        foreach (Transform t in detachOnDestroy)
        {
            t.parent = null;

            // try to get a particle emitter from the detached object
            ParticleEmitter pe = (ParticleEmitter)t.GetComponent("ParticleEmitter");

            // got a particle emitter?
            if (pe)
            {
                // stop emitting particles
                pe.emit = false;
            }
        }

        // destroy the projectile GameObject
        Destroy(gameObject);
    }
}
