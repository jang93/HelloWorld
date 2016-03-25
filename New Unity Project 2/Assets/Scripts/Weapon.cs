using UnityEngine;
using System.Collections;

//! Base class for weapons.
public class Weapon : MonoBehaviour
{
    //! Enabled to log debug messages
    public bool debug;

    //! The type of weapon
    public enum WeaponType { Unarmed, Item };
    // default weapon type is Unarmed
    public WeaponType weaponType = WeaponType.Unarmed;

    //! The Unit using this weapon
    protected Unit unit;

    //! Internal flag to indicate whether this weapon is being used/fired
    bool input;

    //! The icon to use for this weapion in the player HUD
    public Texture icon;

    //! This weapon's rate of fire (seconds)
    public float rateOfFire = 1f;
    //! A random variance applied to the rate of fire (good for pistols and other non-auto weapons)
    public float rateOfFireVariance = 0f;
    //! Internal variable to track time to next fire
    float nextFire;

    //! The maximum attack range of the weapon
    public float range = 100f;
    //! The maximum effective range of the weapon (AIs will close if target is outside this range)
    public float maxRange = 20f;
    //! The minimum effective range of the weapon (AIs will retreat if target is outside this range)
    public float minRange = 10f;

    //! The damage done by this weapon
    public float damage = 10f;

    //! The clip/magazine size of this weapon
    public int maxAmmo = 10;
    //! The weapon's current clip/magazine ammo count (-1 for infinite)
    int ammo;

    //! How long (in seconds) it takes to reload this weapon
    public float reloadTime = 3f;
    //! Internal variable to track time to reload
    float reloadWait;

    //! The random angle applied to this weapon's projectiles.
    public float accuracyError = 2.5f;

    //! The world position that the weapon is aiming at.
    protected Vector3 aimPos = Vector3.zero;

    //! Muzzle flash effect
    public Transform muzzleFlash;
    //! Position that this weapon fires from (for targeting, fire effects, etc)
    public Transform[] firePoints;
    //! Fire effects created when this weapon fires.
    public Transform[] fireEffects;
    //! A flag to indicate whether this weapon should produce hit effects on Units.
    public bool unitHitEffects = true;
    //! Effect created when this weapon hits the world.
    public Transform worldHitEffect;

    //! Name(s) of Component(s) attached to Units when hit by this weapon (e.g. infection)
    public string[] unitAddComponents;
    //! Effects created and attached to Units when hit by this weapon (e.g. fire)
    public Transform[] unitAttachEffects;

    //! Crosshair to use when this weapon is used by the player
    public Texture crosshair;

    //! Sounds played by this weapon when it is fired (one is chosen randomly)
    public AudioClip[] fireSounds;
    //! Sounds played by this weapon when it is reloaded (one is chosen randomly)
    public AudioClip[] reloadSounds;

    //! Start() function.
    public virtual void Start()
    {
        // init current ammo to max ammo
        ammo = maxAmmo;
    }

    //! Update() function.
    public virtual void Update()
    {
        // decrement next fire timer
        nextFire -= Time.deltaTime;

        // is input on and is the weapon ready to fire and does it have ammo?
        if (input && nextFire <= 0f && (ammo == -1 || ammo > 0))
        {
            // fire!
            Fire();
        }

        // is the weapon reloading?
        if (Reloading)
        {
            // decrement reloading time
            reloadWait -= Time.deltaTime;

            // finished reloading
            if (reloadWait <= 0f)
            {
                // set ammo to full
                ammo = maxAmmo;
            }
        }
    }

    //! AimAt() function.  Sets the weapon target position.
    //! @param Vector3 pos  The position to aim at.
    public void AimAt(Vector3 pos)
    {
        // set current aim position
        aimPos = pos;
        // look at the new aim position
        transform.LookAt(pos);
    }

    //! Fire() function.  Handles basic fire operation, including ammo, rate of fire, muzzle flash, fire sounds, etc.
    //! Subclasses should override this function, and call it.  If it returns true, the weapon can fire, otherwise it cannot.
    //! @return bool  True if the weapon fired, false if not.
    protected virtual bool Fire()
    {
        if (debug)
            Debug.Log("WeaponRay.Fire() " + name + " Reloading " + Reloading);

        // is the weapon reloading?
        if (Reloading)
        {
            // can't fire
            return false;
        }

        // is ammo not infinite
        if (ammo != -1)
        {
            // reduce ammo by one shot
            --ammo;
        }

        // does this weapon have a AudioSource and fire sound effects assigned?
        if (GetComponent<AudioSource>() && fireSounds.Length > 0)
        {
            // if the weapon only has one fire sound effect assigned
            if (fireSounds.Length == 1)
            {
                // play it
                GetComponent<AudioSource>().PlayOneShot(fireSounds[0]);
            }
            else
            {
                // play a random fire sound effect
                GetComponent<AudioSource>().PlayOneShot(fireSounds[Random.Range(0, fireSounds.Length)]);
            }
        }

        // reset next fire time
        nextFire = rateOfFire;

        // some rate of variance specified?
        if (rateOfFireVariance > 0f)
        {
            // add random rate of fire variance in +/- range
            nextFire += Random.Range(-rateOfFireVariance, rateOfFireVariance);
        }

        // muzzle flash effect assigned?
        if (muzzleFlash)
        {
            // if a fire point is assigned, use its position for the muzzle flash, otherwise just use weapon's position
            Vector3 pos = (firePoints.Length == 0 ? transform.position : firePoints[0].position);
            // if a fire point is assigned, use its forward for the muzzle flash, otherwise just use weapon's forward
            Vector3 fwd = (firePoints.Length == 0 ? transform.forward : firePoints[0].forward);

            // create the muzzle flash effect
            Instantiate(muzzleFlash, pos, Quaternion.LookRotation(fwd));
        }

        // is the weapon out of ammo?
        if (ammo == 0)
        {
            // reload automatically
            Reload();
        }

        // the weapon fired, so return true
        return true;
    }

    //! Reload() function.  Handles basic reload operation, including ammo, reload sounds, etc
    public void Reload()
    {
        // is the weapon already reloading?
        if (Reloading)
        {
            // do nothing
            return;
        }

        // reset reload timer
        reloadWait = reloadTime;

        // does this weapon have an AudioSource and reload sound effects(s) assigned?
        if (GetComponent<AudioSource>() && reloadSounds.Length > 0)
        {
            // is ony one reload sound effect assigned?
            if (reloadSounds.Length == 1)
            {
                // play it
                GetComponent<AudioSource>().PlayOneShot(reloadSounds[0]);
            }
            else
            {
                // play a random reload sound effect
                GetComponent<AudioSource>().PlayOneShot(reloadSounds[Random.Range(0, reloadSounds.Length)]);
            }
        }
    }

    //! Property to access input variable.  Set true to fire, false to cease fire.
    public bool Input
    {
        get { return input; }
        set { input = value; }
    }

    //! Property to access reloading state.  True when the weapon is reloading.
    public bool Reloading
    {
        get 
        { 
            // return true if the weapon is currently waiting on a reload, otherwise return false
            return (reloadWait > 0f); 
        }
    }

    //! Property to access ammo variable.  Returns current clip/magazine ammo count.
    public int Ammo
    {
        get { return ammo; }
    }

    //! Property to access unit variable.
    public Unit Unit
    {
        get { return unit; }
        set { unit = value; }
    }
}
