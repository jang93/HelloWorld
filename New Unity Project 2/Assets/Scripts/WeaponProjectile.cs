using UnityEngine;
using System.Collections;

//! Weapon subclass implementing weapons based on projectiles (i.e. pulse rifle)
public class WeaponProjectile : Weapon 
{
    //! Projectiles fired by this weapon
    public Transform[] projectiles;

    //! Velocity of projectiles fired by this weapon
    public float projectileVelocity = 10f;
    //! A flag to indicate whether projectiles should use a ballistic arc (grenade, mortar, etc), otherwise they fire in a straight line (pulse rifle)
    public bool ballistic;

    //! Start() function.
    public override void Start()
    {
        // call base Start() function
        base.Start();
    }

    //! Update() function.
    public override void Update()
    {
        // call base Update() function
        base.Update();
    }

    //! Fire() function.
    //! @return bool  True if the weapon fired, false if not.
    protected override bool Fire()
    {
        // call base Fire() function
        if (!base.Fire())
        {
            // base returned false, so this weapon is not ready/able to fire
            return false;
        }

        // does this weapon have any fire points assigned?
        if (firePoints.Length > 0)
        {
            // for each fire point
            foreach (Transform firePoint in firePoints)
            {
                // fire a projectile from this fire point
                FireProjectile(firePoint.position, firePoint.forward);
            }
        }
        else
        {
            // fire a projectile from the weapon's position
            FireProjectile(transform.position, transform.forward);
        }

        // the weapon fired, so return true
        return true;
    }

    //! FireProjectile() function.  Creates the fired projectile.
    //! @param Vector3 pos  The position of the created projectile 
    //! @param Vector3 fwd The forward direction of the created projectile
    void FireProjectile(Vector3 pos, Vector3 fwd)
    {
        // is an accuracy error specified?
        if (accuracyError > 0f)
        {
            // calc a rotation based on the passed fire forward
            Quaternion accRot = Quaternion.LookRotation(fwd);
            // apply a random rotation of +/- accuracy error around the Y axis (randomized fire forward within an arc)
            accRot *= Quaternion.AngleAxis(Random.Range(-accuracyError, accuracyError), Vector3.up);
            // calc the new forward
            fwd = accRot * Vector3.forward;
        }

        // for each projectile fired by this weapon
        foreach (Transform projectile in projectiles)
        {
            // create the projectile at the passed position and (possibly modified above) forward
            Transform p = (Transform)Instantiate(projectile, pos, Quaternion.LookRotation(fwd));

            // null safety check
            if (p)
            {
                // get the projectile Component
                Projectile proj = (Projectile)p.GetComponent("Projectile");

                // null safety check
                if (proj)
                {
                    // set projectile properties
                    proj.damage = damage;
                    proj.velocity = projectileVelocity;
                    proj.unit = unit;
                    proj.hitMask = GameManager.Instance.worldMask | GameManager.Instance.groundMask | unit.enemies;
                }

                // is this projectile ballistic?  (if so, the projectile should have a rigidbody and collider)
                if (ballistic)
                {
                    // launch angle to be calculated
                    float angle = 0f;

                    // pass the target and position to the CalcBallisticLaunchAngle() function
                    if (CalcBallisticLaunchAngle(aimPos, out angle))
                    {
                        // rotate the projectile by the calculated launch angle
                        p.Rotate(new Vector3(-angle, 0, 0));
                    }
                }

            }
        }
    }

    //! CalcBallisticLaunchAngle() function. A utility function to calculate the launch angle needed to hit the target position.
    //! @param Vector3 target  The target position to hit.
    //! @param out float estate  The launch angle required to hit the target position.
    //! @param bool  Returns true if the target position can be hit, given projectile velocity.  Otherwise, returns false.
    //! @return bool  Returns true if the target position can be hit, otherwise returns false.
    bool CalcBallisticLaunchAngle(Vector3 target, out float angle)
    {
        // this is a complicated function to calculate 
        // the required launch angle to hit a target position 
        // with a projectile that moves at a given velocity

        float r = Vector3.Distance(target, transform.position);
        float g = Physics.gravity.y;
        float v = projectileVelocity;

        float b = Mathf.Pow(v, 2);
        float c = Mathf.Pow(v, 4);
        float d = g * Mathf.Pow(r, 2);
        float e = Mathf.Pow(v, 2);
        float f = 2 * target.y * e;
        float h = g * (d + f);
        float i = c - h;

        if (i < 0)
        {
            angle = -1;
            return false;
        }

        float j = Mathf.Sqrt(i);
        float y = b + j;
        float x = g * r;

        angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        angle -= 90f;

        return true;
    }


}
