using UnityEngine;
using System.Collections;

//! A general OnTriggerEnter() handler class which can perform a variety of operations.
public class Trigger : MonoBehaviour
{
    //! Enable to log debug messages.
    public bool debug;

    //! A collide mask for the collision check.
    public LayerMask collidesWith;
    //! If enabled, this trigger will only respond objects with the "Player" tag.
    public bool playerOnly;

    //! Apply damage to detected objects (negative to heal).
    public float damagePerSecond = 0f;
    //! If enabled, damage is scaled over range.
    public bool scaleDamageOverRange = true;
    //! If enabled, damaged objects are told to create damage effects.
    public bool doHitEffects = true;

    //! When triggered, these GameObjects are set active.
    public GameObject[] activateTargets;
    //! When triggered, these Components are enabled.
    public MonoBehaviour[] enableTargets;

    //! When trigged, this Path node is assigned to specified UnitAI targets
    public GameObject assignPathNodeToAITargets;
    //! UnitAI targets.
    public UnitAI[] AItargets;

    //! A Path node to assign to detected UnitAIs and/or those in the AItargets array.
    public GameObject assignPathNodeToDetected;
    //! A Path mode to assign to detected UnitAIs and/or those in the AItargets array.
    public UnitAI.ePathMode pathMode = UnitAI.ePathMode.Once;
    //! Sticky path flag to assign to detected UnitAIs and/or those in the AItargets array.
    public bool stickyPath;

    //! If enabled, GameObjects are destroyed when detected.
    public bool destroyDetectedOnEnter;
    //! Count of destroyed GameObjects.
    public int destroyCount;

    //! If enabled, this Trigger is disabled after it is triggered.
    public bool deactivateOnTrigger;

    //! Effects to create and attach to detected objects.  They are detached/destroyed when the GameObjects leave the trigger.
    public Transform[] attachEffects;
    //! Internal ArrayList to track current attached effects.
    ArrayList activeEffects = new ArrayList();

    //! OnTriggerEnter() function.
    //! @param Collider other  The collider that entered this trigger.
    void OnTriggerEnter(Collider other)
    {
        // what layer(s) to collide with?
        if (((1 << other.gameObject.layer) & collidesWith) == 0)
        {
            return;
        }

        // does this trigger only act on the Player?
        if (playerOnly && other.gameObject.tag != "Player")
        {
            return;
        }

        if (debug)
            Debug.Log("Trigger.OnTriggerEnter() " + name + " detected " + other.name);

        // call the DoDamage() function
        DoDamage(other);

        // attach effects
        foreach (Transform effect in attachEffects)
        {
            // create the effect
            Transform e = (Transform)Instantiate(effect, other.transform.position, Quaternion.identity);
            // attach it to the detected object
            e.parent = other.transform;
            // add it to the array of active attach effects
            activeEffects.Add(e);
        }

        // activate GameObject targets
        foreach (GameObject target in activateTargets)
        {
            // activate the target GameObject
            target.active = true;
        }

        // enable Component targets
        foreach (MonoBehaviour component in enableTargets)
        {
            // enable Component
            component.enabled = true;
        }

        // path node to assign to detected?
        if (assignPathNodeToDetected)
        {
            // try to get a UnitAI Component
            UnitAI unitAI = (UnitAI)other.gameObject.GetComponent("UnitAI");

            // got a UnitAI Component?
            if (unitAI)
            {
                // set path properties
                unitAI.pathNode = assignPathNodeToDetected;
                unitAI.pathMode = pathMode;
                unitAI.stickyPath = stickyPath;
            }
        }

        // path node to assign to specified AI targets?
        if (assignPathNodeToAITargets)
        {
            // AI targets
            foreach (UnitAI ai in AItargets)
            {
                // null safety check (might be destroyed)
                if (ai)
                {
                    // set path properties
                    ai.pathNode = assignPathNodeToAITargets;
                    ai.pathMode = pathMode;
                    ai.stickyPath = stickyPath;
                }
            }
        }
        
        // destroy detected?
        if (destroyDetectedOnEnter)
        {
            // destroy counter
            ++destroyCount;
            // destroy the object
            Destroy(other.gameObject);
        }

        // deactivate trigger on detection?
        if (deactivateOnTrigger)
        {
            // deactivate gameObject
            gameObject.active = false;
        }
    }

    //! OnTriggerStay() function
    //! @param Collider other  The collider that remains in this trigger.
    void OnTriggerStay(Collider other)
    {
        // what layer(s) to collide with?
        if (((1 << other.gameObject.layer) & collidesWith) == 0)
            return;

        // does this trigger only act on the Player?
        if (playerOnly && other.gameObject.tag != "Player")
            return;

        if (debug)
            Debug.Log("Trigger.OnTriggerStay() " + name + " detected " + other.name);

        // call DoDamage() function
        DoDamage(other);
    }

    //! OnTriggerExit() function.
    //! @param Collider other  The collider that exited this trigger.
    void OnTriggerExit(Collider other)
    {
        // check active effects
        for (int i = 0; i < activeEffects.Count; )
        {
            // get the effect from the array
            Transform e = activeEffects[i] as Transform;

            // is this effect attached to the exiting object?
            if (e.parent == other.transform)
            {
                // remove the effect from active effects
                activeEffects.RemoveAt(i);

                // try to get a particle emitter
                ParticleEmitter pe = (ParticleEmitter)e.GetComponent("ParticleEmitter");

                // go a particle emitter?
                if (pe)
                {
                    // stop emitting
                    pe.emit = false;
                }
                else
                {
                    // destroy the effect
                    Destroy(e.gameObject);
                }
            }
            else
            {
                // next object
                ++i;
            }
        }
    }

    //! DoDamage function.  Applies damage to GameObjects in the Trigger.
    //! @param Collider other  The collider that should have damage to apply to it.
    void DoDamage(Collider other)
    {
        // null safety check
        if (!other)
            return;

        // no damage?
        if (damagePerSecond == 0f)
            return;

        // try to get a Damageable from the object
        Damageable damageable = (Damageable)other.transform.GetComponent("Damageable");

        if (damageable)
        {
            Vector3 hitPos = other.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

            float damageScale = 1f;

            if (scaleDamageOverRange)
            {
                damageScale = GetComponent<Collider>().bounds.size.magnitude / Vector3.Distance(transform.position, hitPos);
            }

            float doDamage = damagePerSecond * damageScale * Time.deltaTime;

            damageable.Damage(doDamage, hitPos, (transform.position - hitPos).normalized, null, doHitEffects);
        }
    }

}
