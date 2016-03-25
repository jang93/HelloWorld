using UnityEngine;
using System.Collections;

//! GameManager subclass for the Survival game mode.  Responsible for drawing Survival GUI elements.
public class SurvivalManager : GameManager
{
    //! The current wave number.
    int wave;
    //! The current number of enemies.
    int numEnemies;

    //! References to enemy wave spawns.
    public Transform[] waveSpawns;

    //! Current enemies.
    UnitAI[] enemies;

    //! Delay on wave spawn.
    public float nextWaveWait;
    //! Internal wait on wave spawn.
    float updateWait;

    //! Update function.  Overrides and calls GameManager.Update().
    public override void Update()
    {
        // call base Update()
        base.Update();

        // update wait timer
        updateWait -= Time.deltaTime;
        // next wave wait timer
        nextWaveWait -= Time.deltaTime;

        // time to update yet?
        if (updateWait > 0f)
            return;

        // reset number of enemies
        numEnemies = 0;

        // get UnitAIs from unitRoot children
        enemies = unitRoot.GetComponentsInChildren<UnitAI>();

        // enumerate UnitAIs
        foreach (UnitAI unitAI in enemies)
        {
            // if the AI is not dead, add to number of enemies
            if (!unitAI.Dead)
                ++numEnemies;
        }

        // updating is expensive, so do this only once a second
        updateWait = 1f;

        // ready for next wave?
        if (nextWaveWait > 0f)
        {
            return;
        }

        // next wave when no living enemies remain
        if (numEnemies == 0)
        {
            // increment wave number
            ++wave;
            // reset next wave wait time
            nextWaveWait = 5f;

            // go through the wave spawns, up to the current wave number
            for (int i = 0; i < wave && i < waveSpawns.Length; ++i)
            {
                // get the wave spawn transform
                Transform waveSpawn = waveSpawns[i];

                // go through the children of the wave spawn
                foreach (Transform child in waveSpawn)
                {
                    // is the GameObject already active?
                    if (!child.gameObject.active)
                    {
                        // no, so activate it
                        child.gameObject.SetActiveRecursively(true);
                    }
                    else
                    {
                        // the GameObject is already active, so get the Spawn Components from it
                        Spawn[] spawns = child.GetComponentsInChildren<Spawn>();

                        // re-enable each Spawn 
                        foreach (Spawn spawn in spawns)
                        {
                            spawn.enabled = true;
                        }
                    }
                }
            }
        }
    }

    //! OnGUI function.  Overrides and calls GameManager.OnGUI().
    public override void OnGUI()
    {
        // call base OnGUI() function
        base.OnGUI();

        // custom skin assigned?
        if (guiSkin)
        {
            // assign custom skin
            GUI.skin = guiSkin;
        }
        
        // set GUI color
        GUI.color = Color.white;

        // display current wave number
        GUI.Label(new Rect(0, Screen.height - 40, 100, 20), "Wave: " + wave);

        // display current enemy count
        GUI.Label(new Rect(0, Screen.height - 20, 100, 20), "Enemies: " + numEnemies);

        // has the player died?
        if (player && player.Dead)
        {
            // shot the mouse
            Cursor.visible = true;

            // area for restart controls
            GUILayout.BeginArea(new Rect(Screen.width * 0.2f, Screen.height * 0.25f, Screen.width * 0.6f, Screen.height * 0.5f));

            // eat some vertical space
            GUILayout.FlexibleSpace();

            // You died! title
            GUILayout.Label("You died!", GUI.skin.GetStyle("LabelCentre"));

            // try again button
            if (GUILayout.Button("Try Again"))
            {
                // re-load the survival game scene
                Application.LoadLevel("Survival");
            }

            // main menu button
            if (GUILayout.Button("Main Menu"))
            {
                // load the main menu scene
                Application.LoadLevel("MainMenu");
            }

            // eat remaining vertical space
            GUILayout.FlexibleSpace();

            // end area
            GUILayout.EndArea();
        }
    }
}
