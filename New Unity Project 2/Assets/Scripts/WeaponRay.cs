using UnityEngine;
using System.Collections;

//! Weapon subclass implementing weapons based on ray casts (i.e. pistols, rifles, etc)
public class WeaponRay : Weapon
{
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
                // fire a ray from this fire point
                FireRay(firePoint.position, firePoint.forward);
            }
        }
        else
        {
            // fire a ray from the weapon's position
            FireRay(transform.position, transform.forward);
        }

        // the weapon fired, so return true
        return true;
    }

    //! FireRay() function.  Fires the ray.
    //! @param Vector3 pos  The position of the ray.
    //! @param Vector3 fwd The forward direction of the ray.
    void FireRay(Vector3 pos, Vector3 fwd)
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

        // calc the far hit position
        Vector3 hitPos = pos + (fwd * range);

        // calc the ray
        Ray ray = new Ray(pos, fwd);
        // rayvast hit info
        RaycastHit hit;

        // cast the ray
        if (Physics.Raycast(ray, out hit, range, Unit.enemies | GameManager.Instance.worldMask))
        {
            if (debug)
                Debug.Log ("WeaponRay.FireRay() " + name + " hit " + hit.collider.name);

            // get the hit position
            hitPos = hit.point;

            // get a Damagebale Component from the hit object
            Damageable damageable = (Damageable)hit.collider.gameObject.GetComponent("Damageable");

            // got a Damageable?
            if (damageable)
            {
                // apply damage to the Damageable at the hit position/normal
                damageable.Damage(damage, hit.point, hit.normal, unit, unitHitEffects);

                // does this weapon add components to Units?  (e.g. Infection, etc)
                if (unitAddComponents.Length > 0)
                {
                    // try to get Damageable as a Unit
                    Unit u = damageable as Unit;

                    // got a unit?
                    if (u)
                    {
                        // for each add component
                        foreach (string c in unitAddComponents)
                        {
                            // does this Unit already have this Component added?
                            if (u.gameObject.GetComponent(c) == null)
                            {
                                // no, so add the Component
                                UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(u.gameObject, "Assets/Scripts/WeaponRay.cs (96,33)", c);
                            }
                        }
                    }
                }
            }
            // did not hit a Damageable, so does this weapon have a world hit effect?
            else if (worldHitEffect)
            {
                // create a world hit effect at this hit position/normal
                Instantiate(worldHitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }

        // fire effects
        foreach (Transform fireEffect in fireEffects)
        {
            // create the fire effect at the fire position/forward
            Transform fe = (Transform)Instantiate(fireEffect, pos, Quaternion.LookRotation(fwd));

            // null safety check
            if (fe)
            {
                // try to get a LineRenderer from the fire effect
                LineRenderer lr = (LineRenderer)fe.GetComponent("LineRenderer");

                // got a LineRenderer?
                if (lr)
                {
                    // set the line positions to fire position and hit position
                    lr.SetPosition(0, pos);
                    lr.SetPosition(1, hitPos);
                }
            }
        }

        if (debug)
            Debug.DrawLine(pos, pos + (fwd * range), Color.red);
    }
}
