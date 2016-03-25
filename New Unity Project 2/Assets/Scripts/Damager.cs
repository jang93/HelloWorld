using UnityEngine;
using System.Collections;

//! The Damager class does damage to a Unit that it is attached to.
//! This is useful for "damaging effects", for example, flamethrowers, or healing ranges (which apply negative damage)
public class Damager : MonoBehaviour
{
    //! The Damageable that this Damager will damage, which is found on the parent GameObject
    Damageable damageable;
    
    //! The amount of damage applied per second
    public float damagePerSecond = 10f;

    //! A flag to indicate whether damage effects should be created by the Unit
    public bool doHitEffects;
    
    //! A flag to indicate whether the Damager's GameObject should be destroyed when the Unit is killed
    public bool destroyOnDeath;

    //! Start function
    void Start()
    {
        // Get the Unit attached to the parent GameObject
        damageable = (Damageable)transform.parent.GetComponent("Damageable");

        if (!damageable)
        {
            // if this damager could not get a Damageble, log error and disable 
            Debug.LogError("Damager.Start() " + name + " could not get Damageable from parent " + transform.parent.name + "!");
            enabled = false;
        }
    }

    //! Update function.
    void Update()
    {
        // apply damage to Damageable
        damageable.Damage(damagePerSecond * Time.deltaTime, Vector3.zero, Vector3.forward, null, doHitEffects);

        // check to see if this should be destroyed when the Damageable is dead
        if (destroyOnDeath & damageable.Dead)
        {
            // does this have a particle emitter?
            if (GetComponent<ParticleEmitter>())
            {
                // turn off the emitter
                GetComponent<ParticleEmitter>().emit = false;

                // wait until all the particles have expired
                if (GetComponent<ParticleEmitter>().particles.Length == 0)
                {
                    // destroy this GameObject
                    Destroy(gameObject);
                }
            }
            else
            {
                // destroy this GameObject
                Destroy(gameObject);
            }
        }
    }
}
