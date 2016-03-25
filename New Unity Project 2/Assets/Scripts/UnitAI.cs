using UnityEngine;
using System.Collections;

//! A subclass for Units controlled by artificial intelligence.
public class UnitAI : Unit
{
    //! The awareness range of the Unit, within which it detects other Units
    public float awareRange = 50f;
    //! Internal variable to manage frequency of closest enemy updates
    float closetEnemyUpdate = 1f;
    //! Internal variable to manage time until next closest enemy updates
    float nextclosetEnemyCheck;
    //! Internal reference to closest enemy Unit
    GameObject closestEnemy;

    //! If enabled, this Unit will add enemy Layer to its enemies list when attacked
    public bool addEnemyOnAttack = true;

    //! If greater than zero, this Unit will notify friendly Units within range when attacked
    public float broadcastNewEnemyRange;

    //! Path node for this Unit
    public GameObject pathNode;
    //! Path mode definition
    public enum ePathMode { Once, Loop, PingPong };
    //! Path mode for this Unit
    public ePathMode pathMode = ePathMode.Once;
    //! Path dir for this Unit
    public int pathDir = 1;
    //! A "sticky path" remains active when the Unit is attacked
    public bool stickyPath;

    //! The percent chance thatthis Unit will wander when it doesn't have a target enemy (0 for no wander).
    public float wanderPercent = 50f;
    //! The minimum time in seconds that this Unit will spend wandering in a given direction.
    public float minWanderTime = 5f;
    //! The maximum time in seconds that this Unit will spend wandering in a given direction.
    public float maxWanderTime = 10f;
    //! A point that limits the Unit's wander distance.
    public GameObject tetherPoint;
    //! The maximum distance this Unit will wander from its tether point.
    public float tetherDistance = 5f;
    //! Interval viarible storing this Unit's current wander direction.
    Vector3 wanderDir = Vector3.zero;
    //! Internal variable to keep track of the Unit's current wander time.
    float wanderTime;

    //! Update() function.
    public override void Update()
    {
        // update wander timer
        wanderTime -= Time.deltaTime;

        // call CheckClosestEnemy() function
        CheckClosestEnemy();

        // init look dir to current forward
        Vector3 lookDir = transform.forward;

        // do I have an enemy?
        if (closestEnemy)
        {
            // movement

            // get translation to enemy
            Vector3 delta = (closestEnemy.transform.position - transform.position);
            // zero Y 
            delta.y = 0f;
            // get normalized delta as for dir
            Vector3 dir = delta.normalized;

            // do I have any weapons?
            if (weapons.Count == 0)
            {
                // no, so flee the enemy by negating the dir
                lookDir = move = -dir;
                // run away!
                running = true;
            }
            else
            {
                // I am armed, so look at the enemy
                lookDir = dir;

                // get the distance to the closest enemy
                float enemyDistance = delta.magnitude;

                // weapons
                foreach (Weapon weapon in weapons)
                {
                    // is the enemy is within weapon's specified minimum range?
                    if (weapon.minRange > 0 && enemyDistance < weapon.minRange)
                    {
                        // move backwards
                        move = -dir;
                        // walk
                        running = false;

                        if (debug)
                            Debug.DrawLine(transform.position, closestEnemy.transform.position, Color.red);
                    }
                    // is the enemy within the weapon's specified max (effective) range?
                    else if (weapon.maxRange > 0f && enemyDistance < weapon.maxRange)
                    {
                        // don't move
                        move = Vector3.zero;
                        // walk
                        running = false;

                        if (debug)
                            Debug.DrawLine(transform.position, closestEnemy.transform.position, Color.yellow);
                    }
                    else
                    {
                        // move forward
                        move = dir;
                        // charge!
                        running = true;

                        if (debug)
                            Debug.DrawLine(transform.position, closestEnemy.transform.position, Color.green);
                    }

                    // is the enemy within weapon's range and "in front" of me (less than 30 degrees off forward)?
                    if (enemyDistance <= weapon.range && Vector3.Angle(transform.forward, dir) <= 30f)
                    {
                        // aim the weapon at the enemy
                        weapon.AimAt(closestEnemy.transform.position);
                        // fire!
                        weapon.Input = true;
                    }
                    else
                    {
                        // aim the weapon forward
                        weapon.AimAt(weapon.transform.position + transform.forward);
                        // do not fire
                        weapon.Input = false;
                    }
                }
            }
        }
        else
        {
            // no enemy target

            // walk
            running = false;

            // call CheckNextPathNode() function
            CheckNextPathNode();

            // do I have a move to position?
            if (hasMoveToPosition)
            {
                // get move delta
                Vector3 moveDelta = moveToPosition - transform.position;
                // zero Y 
                moveDelta.y = 0f;

                // is my move target further than 1 meter?
                if (moveDelta.sqrMagnitude > 1f)
                {
                    // look at and move towards move to position
                    lookDir = move = moveDelta.normalized;
                }
                else
                {
                    // don't need to move
                    move = Vector3.zero;
                }

                if (debug)
                    Debug.DrawLine(transform.position, moveToPosition, Color.white);
            }
            // do I have a chance of wandering?
            else if (wanderPercent > 0f)
            {
                // am I not currently wandering?
                if (wanderTime <= 0f)
                {
                    // do I have a tether point, and am I beyond my tether distance?
                    if (tetherPoint && Vector3.Distance(transform.position, tetherPoint.transform.position) > tetherDistance)
                    {
                        // get delta back to my tether point
                        Vector3 tetherDelta = tetherPoint.transform.position - transform.position;
                        // zero Y
                        tetherDelta.y = 0f;

                        // "wander" and look and move back towards my tether point (delta normalized)
                        wanderDir = lookDir = move = tetherDelta.normalized;
                    }
                    // should I wander?  if wander chance == 100% or random % is less than wander chance
                    else if (wanderPercent == 100f || Random.Range(0f, 100f) <= wanderPercent)
                    {
                        // look and move towards and random wander direction
                        lookDir = move = wanderDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
                    }
                    // do not wander
                    else
                    {
                        wanderDir = move = Vector3.zero;
                    }

                    // reset wander timer
                    wanderTime = Random.Range(minWanderTime, maxWanderTime);
                }
                else
                {
                    // continue moving towards current wander (may be zero)
                    move = wanderDir;

                    // if I am moving, look that direction too
                    if (move != Vector3.zero)
                        lookDir = move;
                }
            }
            else
            {
                // do not move
                move = Vector3.zero;
            }

            // I have no enemy, so "reset" my weapons
            foreach (Weapon weapon in weapons)
            {
                // aim it straight forward
                weapon.transform.rotation = weapon.transform.parent.rotation;
                // cease fire
                weapon.Input = false;
            }
        }

        // simple obstacle avoidance

        // following is a very simple obstacle avoidance solution that fires rays forward, slightly left and slightly right, 
        // and attempts to move around any blocking or nearby objects
        // note that this is just simple obstacle avoidance, NOT a more sophisticated path finding solution, so it can easily
        // result in failing to reach move targets, if they are sufficiently blocked, or the AI finds its way into a corner, for example

        // get the angle between my current facing and "desired" look direction, calc'ed above
        float angle = Vector3.Angle(transform.forward, lookDir);

        // is the angle greater than 0?
        if (angle > 0f)
        {
            // turn me towards desired look direction, based on my turn speed
            transform.forward = Vector3.RotateTowards(transform.forward, lookDir, Mathf.Deg2Rad * (turnSpeed * Time.deltaTime), 1f);
            // update angle after turning
            angle = Vector3.Angle(transform.forward, lookDir);
        }
        else
        {
            // I am apparently right on target -- just set my forward directly
            transform.forward = lookDir;
        }
        
        // is the angle greater than 45 degrees?
        if (angle > 45f)
        {
            // if it is, don't move -- this will result in AIs turning in the direction they want to move, before actually moving -- looks better
            move = Vector3.zero;
        }
        else
        {
            // calc rotations for 2 rays, 15 degrees to the left and right of my facing
            Quaternion rightRayRot = transform.rotation * Quaternion.AngleAxis(15f, Vector3.up);
            Quaternion leftRayRot = transform.rotation * Quaternion.AngleAxis(-15f, Vector3.up);

            // calc a direction vector for the rays
            Vector3 rightRayFwd = rightRayRot * Vector3.forward;
            Vector3 leftRayFwd = leftRayRot * Vector3.forward;

            // length of the rays
            float fwdRayLength = 1f;    // forward ray length
            float rightRayLength = 2f;  // right ray length
            float leftRayLength = 2f;   // left ray length

            // ray cast hit info for each ray
            RaycastHit fwdHit;
            RaycastHit rightHit;
            RaycastHit leftHit;

            // cast the rays, storing hit results
            bool fwdBlocked = Physics.Raycast(transform.position, transform.forward, out fwdHit, fwdRayLength, GameManager.Instance.worldMask | friendlies);
            bool rightBlocked = Physics.Raycast(transform.position, rightRayFwd, out rightHit, rightRayLength, GameManager.Instance.worldMask | friendlies);
            bool leftBlocked = Physics.Raycast(transform.position, leftRayFwd, out leftHit, leftRayLength, GameManager.Instance.worldMask | friendlies);

            // any obstacles?
            if (fwdBlocked || rightBlocked || leftBlocked)
            {
                // calc the closest obstacle distance, first by getting the shortest of the forward and right rays...
                float closest = Mathf.Min((fwdBlocked ? fwdHit.distance : fwdRayLength), (rightBlocked ? rightHit.distance : rightRayLength));
                // ... then by getting the shortest of the shortest above and the left ray
                closest = Mathf.Min(closest, (leftBlocked ? leftHit.distance : leftRayLength));

                // if I have no enemy, or if the obstacle is closer than my enemy
                if ( !closestEnemy || (closest < Vector3.Distance (transform.position, closestEnemy.transform.position)) )
                {
                    // is the forward ray blocked?
                    if (fwdBlocked)
                    {
                        // if the left ray is blocked closer than the right ray
                        if (leftBlocked && (!rightBlocked || rightHit.distance >= leftHit.distance))
                        {
                            // slide right
                            move = transform.right;
                        }
                        else
                        {
                            // slide left
                            move = -transform.right;
                        }
                    }
                    // if the left ray is blocked closer than the right ray
                    else if (leftBlocked && (!rightBlocked || rightHit.distance >= leftHit.distance))
                    {
                        // slide fwd/right
                        move = (transform.forward + transform.right).normalized;
                    }
                    // if the right ray is blocked closer than the left ray
                    else if (rightBlocked && (!leftBlocked || leftHit.distance >= rightHit.distance))
                    {
                        // slide fwd/left
                        move = (transform.forward - transform.right).normalized;
                    }
                }
            }

            // draw debug lines
            if (debug)
            {
                if (fwdBlocked)
                    Debug.DrawLine(transform.position, fwdHit.point, Color.red);
                else
                    Debug.DrawLine(transform.position, transform.position + (transform.forward * fwdRayLength), Color.yellow);

                if (rightBlocked)
                    Debug.DrawLine(transform.position, rightHit.point, Color.red);
                else
                    Debug.DrawLine(transform.position, transform.position + (rightRayFwd * rightRayLength), Color.green);

                if (leftBlocked)
                    Debug.DrawLine(transform.position, leftHit.point, Color.red);
                else
                    Debug.DrawLine(transform.position, transform.position + (leftRayFwd * leftRayLength), Color.blue);
            }
        }

        // call base Update()
        base.Update();
    }

    //! Damage() function.  Overrides Damageable.Damage, and calls base.
    //! <param name="damage">The amount of damage to apply.  Can be negative to heal.</param>
    //! <param name="hitPos">The position of the hit.</param>
    //! <param name="hitNormal">The normal (direction) of the hit.</param>
    //! <param name="attacker">The Unit that is causing this damage.</param>
    //! <param name="dodamageEffects">A flag indicating whether this damage should create hit effects.</param>
    public override void Damage(float damage, Vector3 hitPos, Vector3 hitNormal, Unit attacker, bool doHitEffects)
    {
        // call base Damage() function
        base.Damage(damage, hitPos, hitNormal, attacker, doHitEffects);

        // if I am not on a sticky path, null my current path reference
        if (!stickyPath)
            pathNode = null;

        // was an attacker passed?
        if (attacker)
        {
            // if I am not dead and I add attackers to my enemies list
            if (!Dead && addEnemyOnAttack)
            {
                // add the enemy
                AddEnemy(attacker.gameObject);
            }

            // broadcast the enemy attack
            BroadcastNewEnemy(attacker.gameObject);
        }
    }

    //! CheckClosestEnemy() function.  This function finds the closest enemy Unit.
    void CheckClosestEnemy()
    {
        // this function is expensive, so only do it once a second
        nextclosetEnemyCheck -= Time.deltaTime;

        // is it time to check yet?
        if (nextclosetEnemyCheck > 0f)
        {
            // no, so return
            return;
        }

        // do I currently have a target enemy?
        if (closestEnemy)
        {
            // try to get a Unit Component from the enemy
            Unit u = (Unit)closestEnemy.GetComponent("Unit");

            // if I got a Unit that is dead
            if (u && u.Dead)
            {
                // stop targeting this enemy
                closestEnemy = null;
            }

            // if I can't see the enemy
            if (!CanSee(closestEnemy))
            {
                // stop targeting this enemy
                closestEnemy = null;
            }
        }

        // get all enemy colliders within my aware range 
        Collider[] colliders = Physics.OverlapSphere(transform.position, awareRange, enemies);

        // working reference to a detected enemy
        GameObject newEnemy = null;
        // initialize closest enemy, either to my current enemy distance, or -1
        float closest = (closestEnemy ? Vector3.Distance(closestEnemy.transform.position, transform.position) : -1f);

        // go through each detected enemy
        foreach (Collider c in colliders)
        {
            // calc distance to this enemy
            float distance = Vector3.Distance(c.transform.position, transform.position);

            // is it the closest enemy so far, and can I see it?
            if ((closest == -1 || closest > distance) && CanSee(c.gameObject))
            {
                // yep, so update closest distance
                closest = distance;
                // and set the new enemy reference
                newEnemy = c.gameObject;
            }
        }

        // do I have a new enemy?
        if (newEnemy && newEnemy != closestEnemy)
        {
            // set the current enemy reference
            closestEnemy = newEnemy;
            // tell everyone I have a new enemy
            BroadcastNewEnemy(closestEnemy);
        }

        // resest next enemy check timer
        nextclosetEnemyCheck = closetEnemyUpdate;

        if (debug && closestEnemy)
        {
                Debug.Log("UnitAI.CheckClosestEnemy() " + name + " selected " + closestEnemy.name + " at " + closest);
        }
    }

    //! CanSee() function.  This function determines whether the passed GameObject is visible to the Unit (i.e. unblocked by world geometry, within awareness range)
    //! @param GameObject target  The GameObject to check to see if can be seen by this UnitAI.
    //! @return bool  Returns true if the target can be seen, otherwise returns false.
    bool CanSee(GameObject target)
    {
        // null safety check
        if (!target)
        {
            // I can't see a null reference
            return false;
        }

        // is the target outside my aware range?
        if (Vector3.Distance(transform.position, target.transform.position) > awareRange)
        {
            // I can't see that far
            return false;
        }

        // cast a ray to the target and return true if it DOES NOT hit any world geometry
        return (!Physics.Linecast(transform.position, target.transform.position, GameManager.Instance.worldMask));
    }

    //! AddEnemy() Adds the enemy layer to the enemies list, and also sets it to the closest enemy.
    //! @param GameObject enemy  The new enemy.
    void AddEnemy(GameObject enemy)
    {
        // null safety check
        if (!enemy)
        {
            return;
        }

        // verify I can see this enemy
        if (!CanSee(enemy))
        {
            return;
        }

        // if I do not have a target enemy, or this enemy is closer
        if (!closestEnemy || (closestEnemy != enemy && Vector3.Distance(enemy.transform.position, transform.position) < Vector3.Distance(closestEnemy.transform.position, transform.position)))
        {
            // target the attacker
            closestEnemy = enemy;
        }

        // if the attacker is not friendly to me (might happen in accidental friendly fire incident)
        if (enemy.layer != gameObject.layer)
        {
            // add this enemy's layer to my enemies mask
            enemies |= (1 << enemy.layer);
        }
    }

    //! BroadcastNewEnemy() function.  Broadcasts an attack from an enemy to all friendly Units within awareness range.
    //! @param GameObject newEnemy  The new enemy attacking the Unit.
    void BroadcastNewEnemy(GameObject newEnemy)
    {
        // null safety check or I do not broadcast enemies
        if (!newEnemy || broadcastNewEnemyRange == 0f)
        {
            return;
        }

        // get all friendly colliders within enemy broadcast range
        Collider[] colliders = Physics.OverlapSphere(transform.position, broadcastNewEnemyRange, friendlies);

        // go through detected friendlies
        foreach (Collider c in colliders)
        {
            // if this is me (can happen), ignore and continue
            if (c == GetComponent<Collider>())
            {
                continue;
            }

            // try to get UnitAI Component
            UnitAI unitAI = (UnitAI)c.gameObject.GetComponent("UnitAI");

            // got it?
            if (unitAI)
            {
                // add the enemy to my friendlies enemies
                unitAI.AddEnemy(newEnemy);
            }
        }
    }

    //! CheckNextPathNode() function.  A utility function to check/update the current path node on the Unit's path.
    void CheckNextPathNode()
    {
        // am i currently on a path?
        if (!pathNode)
        {
            // no, so return
            return;
        }

        // delta to current path node
        Vector3 delta = pathNode.transform.position - transform.position;
        // zero Y
        delta.y = 0f;

        // is it within 1 meter?
        if (delta.sqrMagnitude < 1f)
        {
            // i'm there!

            // working next path node reference
            GameObject nextPathNode = null;

            // what path mode am I using?
            switch (pathMode)
            {
                // just once
                case ePathMode.Once:

                    // which way am I going?
                    if (pathDir == 1)
                    {
                        // forward, so just get the next child, if there is one
                        if (pathNode.transform.childCount > 0)
                        {
                            nextPathNode = pathNode.transform.GetChild(0).gameObject;
                        }
                    }
                    else
                    {
                        // backwards, so get the parent
                        nextPathNode = GetParentPathNode(pathNode);
                    }
                    
                    break;

                // looping
                case ePathMode.Loop:

                    // which way am I going?
                    if (pathDir == 1)
                    {
                        // forward, so just get the next child, if there is one
                        if (pathNode.transform.childCount > 0)
                        {
                            nextPathNode = pathNode.transform.GetChild(0).gameObject;
                        }

                        // if there is no child node, get the root path node
                        if (!nextPathNode)
                        {
                            nextPathNode = GetRootPathNode(pathNode);
                        }
                    }
                    else
                    {
                        // backwards, so get the parent
                        nextPathNode = GetParentPathNode(pathNode);

                        // if there is no child node, get the tail path node
                        if (!nextPathNode)
                        {
                            nextPathNode = GetTailPathNode(pathNode);
                        }
                    }

                    break;

                // ping ponging
                case ePathMode.PingPong:

                    // which way am I going?
                    if (pathDir == 1)
                    {
                        // forward, so just get the next child, if there is one
                        if (pathNode.transform.childCount > 0)
                        {
                            nextPathNode = pathNode.transform.GetChild(0).gameObject;
                        }

                        // if there is no child node, get the parent path node
                        if (!nextPathNode)
                        {
                            nextPathNode = GetParentPathNode(pathNode);
                            // now, I am going backwards
                            pathDir = -1;
                        }
                    }
                    else
                    {
                        // backwards, so get the parent
                        nextPathNode = GetParentPathNode(pathNode);

                        // if there is no parent node, get the child path node
                        if (!nextPathNode)
                        {
                            if (pathNode.transform.childCount > 0)
                                nextPathNode = pathNode.transform.GetChild(0).gameObject;

                            // now, I am going forwards
                            pathDir = 1;
                        }
                    }

                    break;
            }

            // set the current path node reference to the next path node
            pathNode = nextPathNode;
        }

        // if the path node has been set...
        if (pathNode)
        {
            // update the move to position
            MoveTo = pathNode.transform.position;
        }
        else
        {
            // I have no path node, so no move to position
            hasMoveToPosition = false;
        }
    }

    //! GetNextPathNode() function.  A utility function to get the next path node of the passed path node.  Returns null otherwise.
    //! @param GameObject node  The node to get the next path node for.
    GameObject GetNextPathNode(GameObject node)
    {
        // null safety check
        if (!node)
        {
            return null;
        }

        // does this path node have any children?
        if (node.transform.childCount == 0)
        {
            // no, so return null
            return null;
        }
        
        // get the child node
        Transform c_node = node.transform.GetChild(0);

        // is it a path node?
        if (c_node.gameObject.tag == "PathNode")
        {
            // yes, so return it
            return c_node.gameObject;
        }

        // no next path node found
        return null;
    }


    //! GetParentPathNode() function.  A utility function to get the parent path node of the passed path node.  Returns null otherwise.
    //! @param GameObject node  The node to get the parent for.
    GameObject GetParentPathNode(GameObject node)
    {
        // null safety check
        if (!node)
        {
            return null;
        }

        // does the node have a parent?  is it a path node?
        if (node.transform.parent && node.transform.parent.gameObject.tag == "PathNode")
        {
            // return the parent's GameObject
            return node.transform.parent.gameObject;
        }

        return null;
    }

    //! GetParentPathNode() function.  A utility function to get the root path node of the passed path node.  Returns null otherwise.
    //! @param GameObject node  The node to get the root for.
    GameObject GetRootPathNode(GameObject node)
    {
        // null safety check
        if (!node)
        {
            return null;
        }

        // init "current" node
        GameObject c_root = node;
        // init "next" node as parent of current node
        GameObject n_root = GetParentPathNode(c_root);

        // while there is a next node
        while (n_root)
        {
            // set next as current
            c_root = n_root;
            // set next to parent of current
            n_root = GetParentPathNode(c_root);
        }

        // return current note
        return c_root;
    }

    //! GetParentPathNode() function.  A utility function to get the tail path node of the passed path node.
    //! @param GameObject node  The node to get the tail for.
    GameObject GetTailPathNode(GameObject node)
    {
        // null safety check
        if (!node)
        {
            return null;
        }

        // if this node has no children, then it IS the tail
        if (node.transform.childCount == 0)
        {
            return node;
        }

        // init "current" node
        GameObject c_node = node;
        // init "next" node as transform's child
        GameObject n_node = GetNextPathNode(node);

        // while there is a next node
        while (n_node)
        {
            // set next as current
            c_node = n_node;

            // get the next path node
            n_node = GetNextPathNode(c_node);
        }

        // return current node
        return c_node;
    }

}
