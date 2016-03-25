using UnityEngine;
using System.Collections;

//! The Damageable class allows GameObjects to be damaged/killed/destroyed.
public class Damageable : MonoBehaviour
{
    //! Debug flag.  When enabled, the Damageable prints debug info to the console/log.
    public bool debug;

    //! The Damageable's maximum/default health.
    public float maxHealth = 100f;
    
    //! The Damageable's current health.
    protected float health;

    //! A Shield may be protecting this Damageable.  It is found in Start().
    protected Shield shield;

    //! Effects to create when this Damageable is damaged.  Damage effects are created at the position where the Damageable is hit by attacks.
    public Transform[] damageEffects;
    
    //! Sounds to play when this Damageable is damaged.
    public AudioClip[] damageSounds;
    
    //! Effects to create when this Damageable is killed.    Die effects are created at the Damageable's position when the Damageable is killed.
    public Transform[] dieEffects;
    
    //! A blood puddle to create when this Damageable is hit.  Blood puddles are created at the Damageable's position when the Damageable is damaged.
    //! Snapped to the ground.
    public Transform bloodPuddle;
    
    //! A material to apply to the Damageable when it is killed.
    public Material deadMaterial;
    
    //! A flag to indicate that the Damageable GameObject should be destroyed when it is killed.
    public bool destroyOnDeath;

    //! Start function.  Virtual, so subclasses can override and initialize.
    public virtual void Start()
    {
        // set current health to maximum/default health
        health = maxHealth;

        // find a Shield attached to this GameObject
        shield = GetComponentInChildren<Shield>();
    }

    //! Apply damage to this Damageable.  Virtual, so subclasses can override and react to Damage.
    //! <param name="damage">The amount of damage to apply.  Can be negative to heal.</param>
    //! <param name="hitPos">The position of the hit.</param>
    //! <param name="hitNormal">The normal (direction) of the hit.</param>
    //! <param name="attacker">The Unit that is causing this damage.</param>
    //! <param name="dodamageEffects">A flag indicating whether this damage should create hit effects.</param>
    public virtual void Damage(float damage, Vector3 hitPos, Vector3 hitNormal, Unit attacker, bool dodamageEffects)
    {
        // if I am already dead, ignore this damage
        if (Dead)
            return;

        // retain damage that gets through shield
        float afterShield = damage;

        // do I have a shield, and am I taking (positive) damage?
        if (shield && damage > 0f)
        {
            // apply damage to shield, which will return how much got through
            afterShield = shield.Damage(damage);

            // if the shield took all damage, return
            if (afterShield <= 0f)
                return;
        }

        // apply damage to health
        health -= afterShield;

        // clamp health to acceptable bounds
        health = Mathf.Clamp(health, 0f, maxHealth);

        // create hit effects?
        if (dodamageEffects)
        {
            // create hit effects at hit position/normal
            foreach (Transform he in damageEffects)
                Instantiate(he, hitPos, Quaternion.LookRotation(hitNormal));

            // create blood puddle at hit position, y = 0 (ground), random rotation around y
            if (bloodPuddle)
                Instantiate(bloodPuddle, new Vector3(transform.position.x, 0f, transform.position.z), Quaternion.AngleAxis(Random.Range(0, 360f), Vector3.up));
        }

        // damage sounds
        if (afterShield > 0f && GetComponent<AudioSource>() != null && damageSounds.Length > 0)
        {
            if (damageSounds.Length == 1)
                GetComponent<AudioSource>().PlayOneShot(damageSounds[0]);
            else
                GetComponent<AudioSource>().PlayOneShot(damageSounds[Random.Range(0, damageSounds.Length)]);
        }

        // am I dead?
        if (health <= 0f)
            Die();
    }

    //! Die function.  Virtual, so subclasses can be notified when they die.
    public virtual void Die()
    {
        // set health to 0
        health = 0f;

        // add "(Dead)" to my name.  Just handy for editor debugging, counting the dead, etc
        transform.name += "(Dead)";

        // disable map blip
        Transform mapBlip = transform.Find("MapBlip");
        if (mapBlip)
            mapBlip.gameObject.active = false;

        // set rigidbody free
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().freezeRotation = false;
        }

        // set my layer mask to "Dead"
        if (gameObject.layer != LayerMask.NameToLayer("World"))
            gameObject.layer = LayerMask.NameToLayer("Dead");

        // do I have a dead material?
        if (deadMaterial)
        {
            // apply dead material to my renderer, if I have one
            if (GetComponent<Renderer>())
                GetComponent<Renderer>().material = deadMaterial;
            else
            {
                // otherwise, apply to child renderers
                Renderer[] renderers = GetComponentsInChildren<Renderer>();

                foreach (Renderer r in renderers)
                    r.material = deadMaterial;
            }
        }

        // create die effects
        foreach (Transform dieEffect in dieEffects)
            Instantiate(dieEffect, transform.position, transform.rotation);

        // if I should be destroyed, do it, otherwise just disable this script
        if (destroyOnDeath)
            Destroy(gameObject);
        else
            enabled = false;
    }

    //! Property to check dead state.
    public bool Dead
    {
        // if health is <= zero, return true, else return false
        get { return (health <= 0f); }
    }

    //! Property to get health.
    public float Health
    {
        get { return health; }
    }

}
