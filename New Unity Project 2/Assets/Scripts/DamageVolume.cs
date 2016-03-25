using UnityEngine;
using System.Collections;

//! Damages objects with a sphere with a radius of range
public class DamageVolume : MonoBehaviour
{
    //! Debug flag.  Enable to log debug messages.
    public bool debug;

    //! The radius of the sphere.
    public float range = 5f;
    //! Damage to be applied.  Can be negative to heal.
    public float damage = 100f;

    //! A flag to indicate whether the damaged object should create damage effects.
    public bool doDamageEffects = true;

    //! A flag to indicate whether damage should be scaled (down) over range.
    public bool scaleDamageOverRange = true;

    //! If this is enabled, the DamageVolume is disabled after its first application.
    public bool oneShot = true;
    //! A delay on the damage application.
    public float damageDelay;
    //! Internal variable to track time until next damage (if applicable).
    float nextDamage;

    //! A mask to apply to the sphere check.
    public LayerMask damageMask;

    //! Start function.
    void Start()
    {
        // init next damage counter
        nextDamage = damageDelay;
    }

    //! Update function.
    void Update()
    {
        // decrement next damage counter
        nextDamage -= Time.deltaTime;

        // time to damage?
        if (nextDamage > 0f)
            return;

        // apply damage
        DoDamage();
    }

    //! The DoDamage function actually applies damage to objects within range.
    void DoDamage()
    {
        // get all colliders in range
        Collider[] colliders = Physics.OverlapSphere(transform.position, range, damageMask);

        // a couple of variables predeclared
        float damageScale = 1f;
        Vector3 hitPos;

        // go through detected colliders
        foreach (Collider c in colliders)
        {
            // get Damagable from collider
            Damageable damageable = (Damageable)c.transform.GetComponent("Damageable");

            // got Damageable?
            if (damageable)
            {
                // get hit position
                hitPos = c.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

                // scale damage by range?
                if (scaleDamageOverRange)
                {
                    // calc damage scalar
                    damageScale = 1f - Vector3.Distance(transform.position, hitPos) / range;
                }

                // scale damage
                float doDamage = damage * damageScale;

                if (debug)
                    Debug.Log("DamageVolume.DoDamage() " + name + " doing " + doDamage + " damage to " + damageable.name);

                // apply damage
                damageable.Damage(doDamage, hitPos, (hitPos - transform.position).normalized, null, doDamageEffects);
            }
        }

        // is this a one shot damage volume?
        if (oneShot)
        {
            // disable this
            enabled = false;
        }
        else
        {
            // reset next damage counter
            nextDamage = damageDelay;
        }
    }
}
