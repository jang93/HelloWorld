using UnityEngine;
using System.Collections;

//! Weapon subclass implementing weapons based on particle effects (i.e. flamethrower)
public class WeaponParticles : Weapon
{
    //! The particle emitter on this weapon
    ParticleEmitter pe;

    //! Start() function.
    public override void Start()
    {
        // call base Start() function
        base.Start();

        // ensure the weapon has a particle emitter
        if (GetComponent<ParticleEmitter>())
        {
            // get particle emitter on this GameObject
            pe = GetComponent<ParticleEmitter>();
        }
        else
        {
            // try to get a particle emitter from children
            pe = GetComponentInChildren<ParticleEmitter>();
        }

        // found particle emitter?
        if (!pe)
        {
            // log a warning
            Debug.LogWarning("WeaponPaticle.Start() " + name + " could not find particleEmitter!");
            // disable this Component
            enabled = false;
        }
    }

    //! Update() function.
    public override void Update()
    {
        // call base Update() function
        base.Update();

        // set the particle emitter to emit based on weapon input
        pe.emit = Input;
    }

    //! OnParticleCollision() calback function.
    //! @param GameObject other  The GameObject hit by the particle.
    void OnParticleCollision(GameObject other)
    {
        if (debug)
            Debug.Log("WeaponPaticle.OnParticleCollision() " + name + " hit " + other.name);

        // does this weapon do damage?  (negative damage could be a "healing" weapon
        if (damage != 0f)
        {
            // look for a Damageable Component on the hit object
            Damageable damageable = (Damageable)other.GetComponent("Damageable");

            // got a Damageable?
            if (damageable)
            {
                // aplly damage to the Damageable
                damageable.Damage(damage, other.transform.position, other.transform.forward, unit, unitHitEffects);
            }
        }

        // does this weapon attach effects to Units?
        if (unitAttachEffects.Length > 0)
        {
            // try to get a Unit from the hit object
            Unit u = (Unit)other.transform.GetComponent("Unit");

            // got a Unit?
            if (u)
            {
                // for each attach effect
                foreach (Transform e in unitAttachEffects)
                {
                    // make sure this effect isn't already attached to the Unit (e.g. fire)
                    if (!Util.GetChildByName(u.transform, e.name, true))
                    {
                        // create the effect at the Unit's position
                        Transform effect = (Transform)Instantiate(e, u.transform.position, Quaternion.identity);

                        // attach it to the Unit's Transform
                        effect.parent = u.transform;
                    }
                }
            }
        }
    }
    
}
