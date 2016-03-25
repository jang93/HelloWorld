using UnityEngine;
using System.Collections;

//! A subclass for the Unit controlled by the player.
public class UnitPlayer : Unit
{
    //! The GUISkin to use in the player GUI (HUD, etc)
    public GUISkin guiSkin;

    //! The player's HUD health bar icon
    public Texture healthIcon;
    //! The player's HUD health bar screen rect
    protected Rect healthBarRect = new Rect(20, 10, 0, 0);
    //! The player's HUD health bar art
    public Texture healthBarTop;
    //! The player's HUD health bar bottom art
    public Texture healthBarBottom;

    //! Start() function.
    public override void Start()
    {
        // call base Start() function
        base.Start();

        // health bar textures assigned?
        if (healthIcon && healthBarTop && healthBarBottom)
        {
            // calc health bar screen rect based on texture size
            healthBarRect = new Rect(20 + healthIcon.width, 10, healthBarTop.width, healthBarTop.height);
        }
    }

    //! Update() function.
    public override void Update()
    {
        // if the player has died...
        if (Dead)
        {
            // release mouse
            Screen.lockCursor = false;
            // disable script
            enabled = false;
            return;
        }

        // make sure there is a GameManager
        if (!GameManager.Instance)
        {
            return;
        }

        // if the game is paused, return
        if (GameManager.Instance.Paused)
        {
            return;
        }

        // if the player has a weapon that has a crosshair
        if (weapons.Count > 0 && (weapons[0] as Weapon).crosshair)
        {
            // hide the mouse cursor (will use the crosshair)
            Cursor.visible = false;
        }
        else
        {
            // show the mouse cursor
            Cursor.visible = true;
        }

        // rotation

        // get a ray at the current mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // raycast hit info
        RaycastHit hit;
        // init look position
        Vector3 lookPos = Vector3.zero;

        // cast a ray to hit the ground or an enemy at the current mouse position
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, GameManager.Instance.groundMask | enemies))
        {
            // if the camera ray hit the ground
            if ((1 << hit.collider.gameObject.layer & GameManager.Instance.groundMask) > 0)
            {
                // look at the hit position
                lookPos = hit.point;
            }
            // if the camera ray hit something else (an enemy)
            else
            {
                // look at the enemy
                lookPos = hit.collider.transform.position;
            }

            // look at the look at position
            transform.LookAt(new Vector3(lookPos.x, transform.position.y, lookPos.z));
        }
        else
        {
            // the ray hit nothing (possibly "outside" the world), so just look at a position 100 meters in front of the player unit
            lookPos = transform.position + transform.forward * 100f;

            if (debug)
                Debug.LogWarning("UnitPlayer.Update() " + name + " could not find ground at mouse position!");
        }

        // movement

        // init move to zero
        move = Vector3.zero;

        // up?
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            // move up (Z+)
            move += Vector3.forward;
        }
        // down?
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            // move down (Z-)
            move -= Vector3.forward;
        }
        // right?
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            // move right (X+)
            move += Vector3.right;
        }
        // left?
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            // move left (X-)
            move -= Vector3.right;
        }

        // running?
        running = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        // pick up?
        if (pickup && Input.GetKeyDown(KeyCode.E))
        {
            // there is a pickup object detected, and the pick up key is held down

            // if I have a weapon...
            if (weapons.Count > 0)
            {
                // drop it
                Drop(weapons[0] as Weapon);
            }

            // pick up the current pickup item
            Pickup(pickup);

            // null the pickup object
            pickup = null;
        }

        // weapon

        // for each weapon...
        foreach (Weapon weapon in weapons)
        {
            // look at the current look at position
            weapon.transform.LookAt(lookPos);

            // set weapon input based on left mouse button state
            weapon.Input = Input.GetMouseButton(0);

            // reload key down?
            if (Input.GetKey(KeyCode.R))
            {
                // reload the weapon
                weapon.Reload();
            }
        }

        // base Update() function
        base.Update();
    }

    //! OnGUI() function.
    public virtual void OnGUI()
    {
        // custom GUISkin assigned?
        if (guiSkin)
        {
            // assign custom GUISkin
            GUI.skin = guiSkin;
        }

        // HUD

        // health

        // health textures assigned?
        if (healthIcon && healthBarTop)
        {
            // draw health icon
            GUI.DrawTexture(new Rect(10, 10, healthIcon.width, healthIcon.height), healthIcon);

            // group for health bar area
            GUI.BeginGroup(healthBarRect);

            // health bar bottom texture assigned?
            if (healthBarBottom)
            {
                // draw the health bar bottom texture (which goes "under" the health bar)
                GUI.DrawTexture(new Rect(0, 0, healthBarBottom.width, healthBarBottom.height), healthBarBottom);
            }

            // group for health bar, width scaled by current health to clip health bar
            GUI.BeginGroup(new Rect(0, 0, healthBarTop.width * (health / maxHealth), healthBarTop.height));

            // draw the health bar top, which has been clipped by current health
            GUI.DrawTexture(new Rect(0, 0, healthBarTop.width, healthBarTop.height), healthBarTop);

            // end health bar top group
            GUI.EndGroup();
            // end health bar bottom group
            GUI.EndGroup();
        }
        else
        {
            // no health bar textures assigned, so just put a text label up
            GUILayout.Label("Health: " + (int)health);
        }

        // shield

        // do I have a shield?
        if (shield)
        {
            // shield textures assigned?
            if (shield.shieldIcon && shield.shieldBarTop && shield.shieldBarBottom)
            {
                // draw the shield icon beside the health bar
                GUI.DrawTexture(new Rect(healthBarRect.x + healthBarRect.width + 10, 10, shield.shieldIcon.width, shield.shieldIcon.height), shield.shieldIcon);

                // group for shield bar area
                GUI.BeginGroup(new Rect(healthBarRect.x + healthBarRect.width + 10 + shield.shieldIcon.width + 10, 10, shield.shieldBarTop.width, shield.shieldBarTop.height));

                // shield bar bottom texture assigned?
                if (shield.shieldBarBottom)
                {
                    // draw the shield bar bottom texture (which goes "under" the shield bar)
                    GUI.DrawTexture(new Rect(0, 0, shield.shieldBarBottom.width, shield.shieldBarBottom.height), shield.shieldBarBottom);
                }

                // group for shield bar, width scaled by current shield strength to clip shield bar
                GUI.BeginGroup(new Rect(0, 0, shield.shieldBarTop.width * (shield.ShieldHealth / shield.maxShieldHealth), shield.shieldBarTop.height));

                // draw the shield bar top, which has been clipped by current shield strength
                GUI.DrawTexture(new Rect(0, 0, shield.shieldBarTop.width, shield.shieldBarTop.height), shield.shieldBarTop);

                // end shield bar top group
                GUI.EndGroup();
                // end shield bar bottom group
                GUI.EndGroup();
            }
            else
            {
                // no shield bar textures assigned, so just put a text label up
                GUILayout.Label("Shield: " + (int)shield.ShieldHealth);
            }

        }

        // weapon

        // do I have a weapon?
        if (weapons.Count > 0)
        {
            // get the player's weapon
            Weapon weapon = weapons[0] as Weapon;

            // does the weapon have an icon texture assigned?
            if (weapon.icon)
            {
                // draw the weapon icon in the upper right of the screen
                GUI.DrawTexture(new Rect(Screen.width - weapon.icon.width - 10, 10, weapon.icon.width, weapon.icon.height), weapon.icon);
                // weapon state -- if it is reloading, print '--', otherwise print the current ammo, and then the full clip size (i.e. '14/30')
                GUI.Label(new Rect(Screen.width - weapon.icon.width - 120, 10, 100, 40), weapon.name + "\n" + (weapon.Reloading ? "--" : weapon.Ammo.ToString()) + "/" + weapon.maxAmmo, GUI.skin.GetStyle("LabelRight"));
            }
            else
            {
                // no weapon icon, so just print the weapon state -- if it is reloading, print '--', otherwise print the current ammo, and then the full clip size (i.e. '14/30')
                GUI.Label(new Rect(Screen.width - 100, 10, 100, 40), weapon.name + "\n" + (weapon.Reloading ? "--" : weapon.Ammo.ToString()) + "/" + weapon.maxAmmo, GUI.skin.GetStyle("LabelRight"));
            }

            // weapon crosshair texture assigned?
            if (weapon.crosshair)
            {
                // calc to centre the crosshair texture on the current mouse position
                Rect crosshairRect = new Rect(Input.mousePosition.x - (weapon.crosshair.width * 0.5f), (Screen.height - Input.mousePosition.y) - (weapon.crosshair.height * 0.5f), weapon.crosshair.width, weapon.crosshair.height);
                // draw the crosshair
                GUI.DrawTexture(crosshairRect, weapon.crosshair);
            }
        }

        // pick up

        // do I have a current pick up object?
        if (pickup)
        {
            // set GUI text color
            GUI.color = Color.white;

            // do I have any weapons at the moment?
            if (weapons.Count > 0)
            {
                // get current weapon
                Weapon weapon = weapons[0] as Weapon;
                // swap weapon prompt
                GUI.Label(new Rect(Screen.width * 0.5f - 100f, Screen.height * 0.2f, 200f, 20f), "Press E to swap " + weapon.name + " for " + pickup.name, GUI.skin.GetStyle("LabelCentre"));
            }
            else
            {
                // pick up weapon prompt
                GUI.Label(new Rect(Screen.width * 0.5f - 100f, Screen.height * 0.2f, 200f, 20f), "Press E to pick up " + pickup.name, GUI.skin.GetStyle("LabelCentre"));
            }
        }

    }

    //! Die() function.
    public override void Die()
    {
        // call base Die() function
        base.Die();

        // unlock the mouse cursor
        Screen.lockCursor = false;
    }

}
