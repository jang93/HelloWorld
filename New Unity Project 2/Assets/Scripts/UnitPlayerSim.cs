using UnityEngine;
using System.Collections;

//! A subclass that extends the UnitPlayer class in order to add the infection bar to the player health bar in the Simulation game mode.
public class UnitPlayerSim : UnitPlayer
{
    //! HUD infection bar icon
    public Texture infectionIcon;
    //! HUD infection bar art
    public Texture infectionBarTop;

    //! Internal reference to the Infection Component attached to this player Unit
    Infection infection;

    //! OnGUI() function.
    public override void OnGUI()
    {
        // call base OnGUI() function
        base.OnGUI();

        // check to see if I am infected 
        if (!infection)
        {
            // try to get an Infection Component
            infection = (Infection)GetComponent("Infection");
        }

        // am I infected?
        if (infection)
        {
            // infection bar is dependent on health bar position/size
            if (infectionBarTop && healthIcon && healthBarTop)
            {
                // define group for the infection bar, clipped by current infection
                GUI.BeginGroup(new Rect(healthBarRect.x, healthBarRect.y, infectionBarTop.width * (infection.CurrentInfection / maxHealth), infectionBarTop.height));

                // draw the infection bar -- this will make it appear "on top" of the health
                GUI.DrawTexture(new Rect(0, 0, infectionBarTop.width, infectionBarTop.height), infectionBarTop);

                // end the infection bar group
                GUI.EndGroup();

                // infection icon assigned?
                if (infectionIcon)
                {
                    // draw the infection icon to the right of the health bar
                    GUI.DrawTexture(new Rect(healthBarRect.x + 10 + healthBarTop.width + 10, 10, infectionIcon.width, infectionIcon.height), infectionIcon);
                }
            }
            else
            {
                // no infection textures assigned, so just put a label up
                GUILayout.Label("Infection: " + (int)infection.CurrentInfection);
            }
        }
    }
}
