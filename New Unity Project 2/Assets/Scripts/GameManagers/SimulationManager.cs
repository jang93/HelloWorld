using UnityEngine;
using System.Collections;

//! GameManager subclass for the Simulation game mode.  Responsible for drawing Simulation GUI elements, and tracking certain game events.
public class SimulationManager : GameManager
{
    //! Reference to the zombie prefab, spawned by infected civilians when they die.
    public Transform zombiePrefab;

    //! Current number of civilians.
    int numCivilians;
    //! Current number of cops.
    int numCops;
    //! Current number of infected.
    int numInfected;
    //! Current number of zombies.
    int numZombies;
    //! Current number of dead.
    int numDead;
    //! Current number of escaped.
    int numEscaped;
    //! Current total population (living + dead + zombies).
    int totalPop;
    //! Current living population.
    int totalLivingPop;

    //! Current percentage of population that are civilians.
    int percentCivilians = 100;
    //! Current percentage of population that are cops.
    int percentCops;
    //! Current percentage of population that are infected.
    int percentInfected;
    //! Current percentage of population that are zombies.
    int percentZombies;
    //! Current percentage of population that are dead.
    int percentDead;
    //! Current percentage of population that have escaped.
    int percentEscaped;

    //! Flag to indicate whether the player is armed (for weapon pick up message).
    bool playerArmed;
    //! Flag to indicate whether the player has escaped armed (for escaped/win message).
    bool playerEscaped;

    //! Reference to the civilian escape trigger, to count escapes. 
    public Trigger civlianEscapeTrigger;
    //! Reference to the Army Unit Spawn.
    public GameObject armyDispatchSpawn;

    //! HUD PDA texture.
    public Texture pda;
    //! HUD PDA screen rect.
    public Rect pdaTextRect = new Rect(24, 260, 78, 148);
    //! HUD PDA message sound.
    public AudioClip pdaSound;

    //! Internal variable to track time until next statistics update.
    float nextStatUpdate;

    //! Internal variable to track elapsed simulation time.
    float simTimeElapsed;

    //! PDA message types.
    public enum ePDAMessageID
    {
        TipMessage1 = 0,
        TipMessage2,
        TipMessage3,
        TipMessage4,
        TipMessage5,
        TipMessage6,
        TipMessageLast,
        ArmyDispatchMessage,
        ArmyPatrolMessage,
        PlayerGotWeaponMessage,
    }

    //! PDA HUD messages
    string[] pdaMessages = 
    {
        "* BEGIN ENCRYPTED COMMS *\n\nUse W,S,A,D or the Arrow Keys to move!\n\n1/7",
        "A Zombie Outbreak has started near the centre of the City!\n\n2/7",
        "Avoid being attacked by Zombies at all costs!\n\nInfection from wounds is fatal!\n\n3/7",
        "Arm yourself if possible!\n\nFind a Cop and grab his gun if he dies!\n\n4/7",
        "Spreading infection will lead to increased police/\nmilitary response!\n\n5/7",
        "Watch your PDA for current situation report (SIT-REP) and information!\n\n6/7", 
        "Good luck!\n\n7/7\n\n* END ENCRYPTED COMMS *",
        "The Army has arrived!\n\nThey will be guarding the East Bridge out of the City!",
        "Army fire teams have begun patrolling the city!",
        "You have found a weapon!\n\nUse the mouse to aim, and left mouse button to shoot!\n\nPress R to reload.",
    };

    //! Current PDA message.
    ePDAMessageID pdaMessage;
    //! Current PDA message time.
    float pdaMessageTimeElapsed;
    //! How long each PDA message is displayed.
    public float pdaMessageTime = 8f;

    //! Percent population at which the Army Units are spawned.
    public int armyDispatchThreshold = 70;
    //! Percent population at which the Army Units begin to patrol the city.
    public int armyPatrolThreshold = 30;

    //! Start function.  Overrides and calls GameManager.Start().
    public override void Start()
    {
        // call base class Start() function
        base.Start();

        // set first PDA message
        ShowPdaMessage(ePDAMessageID.TipMessage1);
    }

    //! Update function.  Overrides and calls GameManager.Update().  Updates Simulation statistics.
    public override void Update()
    {
        // call base class Update() function
        base.Update();

        // update elapsed sim time
        simTimeElapsed += Time.deltaTime;

        // displaying PDA message?
        if (pdaMessageTimeElapsed < pdaMessageTime)
        {
            // age current PDA message
            pdaMessageTimeElapsed += Time.deltaTime;

            // current message expired?
            if (pdaMessageTimeElapsed >= pdaMessageTime)
            {
                // still displaying start up messages?
                if (pdaMessage < ePDAMessageID.TipMessageLast)
                {
                    // display next PDA message
                    ShowPdaMessage(++pdaMessage);
                }
            }
        }

        // update next statistics update timer
        // update statistics is an expensive operation, so this only does it once a second
        nextStatUpdate -= Time.deltaTime;

        // time to update statistics?
        if (nextStatUpdate <= 0f)
        {
            // reset statistics
            numZombies = numCivilians = numCops = numInfected = numDead = 0;

            // iterate through all Unit GameObjects by going through children of the unitRoot GameObject
            for (int i = 0; i < unitRoot.transform.childCount; ++i)
            {
                // try to get the Unit Component
                Unit unit = (Unit)unitRoot.transform.GetChild(i).GetComponent("Unit");

                // got a Unit Component?
                if (unit)
                {
                    // is it a living civilian?
                    if (unit.name.StartsWith("Civilian") && !unit.Dead)
                    {
                        // count civilian
                        ++numCivilians;
                    }
                    // is it a living cop?
                    else if (unit.name.StartsWith("Cop") && !unit.Dead)
                    {
                        // count cop
                        ++numCops;
                    }
                    // is it a zombie?
                    else if (unit.name.StartsWith("Zombie") && !unit.Dead)
                    {
                        // count zombie
                        ++numZombies;
                    }
                    // is it a dead Unit?
                    else if (unit.Dead)
                    {
                        // count dead
                        ++numDead;
                    }

                    // is the Unit infected?
                    if (unit.GetComponent("Infection"))
                    {
                        // count infected
                        ++numInfected;
                    }
                }
            }

            // is the civilianEscapeTrigger assigned?
            if (civlianEscapeTrigger)
            {
                // get the number of escaped civilians (destroyed by civilianEscapeTrigger)
                numEscaped = civlianEscapeTrigger.destroyCount;
            }

            // total living population is civilians + cops
            totalLivingPop = numCivilians + numCops;
            // total population is living plus zombies
            totalPop = totalLivingPop + numZombies;

            // are all numbers positive?
            if (totalPop > 0 && totalLivingPop > 0)
            {
                // store current civilian percent 
                int curPercentCivilians = percentCivilians;

                // update all percents
                percentCivilians = (int)(((float)numCivilians / (float)totalPop) * 100f);
                percentCops = (int)(((float)numCops / (float)totalPop) * 100f);
                percentInfected = (int)(((float)numInfected / (float)totalLivingPop) * 100f);
                percentZombies = (int)(((float)numZombies / (float)totalPop) * 100f);
                percentDead = (int)(((float)numDead / (float)(totalPop + numDead)) * 100f);
                percentEscaped = (int)(((float)numEscaped / (float)(totalPop + numEscaped)) * 100f);

                // passed the army dispatch threshold?
                if (curPercentCivilians > armyDispatchThreshold && percentCivilians <= armyDispatchThreshold)
                {
                    // display army dispatch PDA message
                    ShowPdaMessage(ePDAMessageID.ArmyDispatchMessage);

                    // has the army dispatch GameObject been assigned?
                    if (armyDispatchSpawn)
                    {
                        // let slip the dogs of war!
                        armyDispatchSpawn.SetActiveRecursively(true);
                    }
                }
                // passed the army patrol threshold?
                else if (curPercentCivilians > armyPatrolThreshold && percentCivilians <= armyPatrolThreshold)
                {
                    // display army patrol PDA message
                    ShowPdaMessage(ePDAMessageID.ArmyPatrolMessage);

                    // begin army patrols
                    BeginArmyPatrols();
                }
            }

            // reset next statistics update timer
            nextStatUpdate = 1f;
        }

        // has the player armed?
        if (player && !playerArmed && player.Weapons.Count > 0)
        {
            // display player armed message
            ShowPdaMessage(ePDAMessageID.PlayerGotWeaponMessage);
            // set player armed flag
            playerArmed = true;
        }
    }

    //! OnGUI function.  Overrides and calls GameManager.OnGUI().  Draws the PDA.
    public override void OnGUI()
    {
        // this does not call the base OnGUI() function because the pause menu is displayed in the PDA

        // custom GUISkin specified?
        if (guiSkin)
        {
            // assigne custom GUISkin
            GUI.skin = guiSkin;
        }

        // has the PDA texture been assigned?
        if (pda)
        {
            // draw PDA texture in the lower left corner
            GUI.DrawTexture(new Rect(0, Screen.height - pda.height, pda.width, pda.height), pda);
        }

        // begin an area for the PDA display contents
        GUILayout.BeginArea(pdaTextRect);

        // if the player is null (destroyed), s/he has either escaped or died
        if (!player)
        {
            // show the mouse
            Cursor.visible = true;

            // set the GUI color
            GUI.color = Color.green;

            // the playerEscaped flag is set by the trigger on this GameObject
            if (playerEscaped)
            {
                // escaped message
                GUILayout.Label("You escaped!", GUI.skin.GetStyle("PDA"));
            }
            // otherwise, the player was killed
            else
            {
                // died message
                GUILayout.Label("You died!", GUI.skin.GetStyle("PDA"));
            }

            // vertical space
            GUILayout.Space(20);

            // play again
            if (GUILayout.Button("Restart", GUI.skin.GetStyle("PDAButton")))
            {
                // reload the level
                Application.LoadLevel("Simulation");
            }

            // return to main menu
            if (GUILayout.Button("Main Menu", GUI.skin.GetStyle("PDAButton")))
            {
                // load main menu
                Application.LoadLevel("MainMenu");
            }
        }
        // paused?
        else if (Paused)
        {
            // show cursor
            Cursor.visible = true;

            // set GUI color
            GUI.color = Color.green;

            // paused title
            GUILayout.Label("PAUSED", GUI.skin.GetStyle("LabelCentre"));

            // some vertical space
            GUILayout.Space(20);

            // resume game
            if (GUILayout.Button("Resume Game", GUI.skin.GetStyle("PDAButton")))
            {
                // unpause
                Paused = false;
            }

            // return to main menu button
            if (GUILayout.Button("Main Menu", GUI.skin.GetStyle("PDAButton")))
            {
                // load the main menu
                Application.LoadLevel("MainMenu");
            }
        }
        // time to show next PDA message?
        else if (pdaMessageTimeElapsed < pdaMessageTime)
        {
            // this flashes the message every 1/2 second for 3 seconds
            if (pdaMessageTimeElapsed < 2f && (pdaMessageTimeElapsed - (int)pdaMessageTimeElapsed) > 0.5f)
            {
                // blank message for flashing
                GUILayout.Label("");
            }
            else
            {
                // set GUI color
                GUI.color = Color.green;
                // display current PDA message
                GUILayout.Label(pdaMessages[(int)pdaMessage], GUI.skin.GetStyle("PDA"));
            }
        }
        // has the zombie infection been beaten?
        else if (numZombies == 0 && numInfected == 0)
        {
            // set GUI color
            GUI.color = Color.green;
            // victory message
            GUILayout.Label("The zombie infection has been contained!", GUI.skin.GetStyle("PDA"));

            // some vertical space
            GUILayout.Space(20);

            // play again button
            if (GUILayout.Button("Play Again", GUI.skin.GetStyle("PDAButton")))
            {
                // reload this level
                Application.LoadLevel("Simulation");
            }

            // main menu button
            if (GUILayout.Button("Main Menu", GUI.skin.GetStyle("PDAButton")))
            {
                // return to main menu
                Application.LoadLevel("MainMenu");
            }
        }
        // still going
        else
        {
            // get sim elapsed minutes
            int mins = (int)(simTimeElapsed / 60f);
            // get sim elapsed seconds
            int secs = (int)(simTimeElapsed % 60f);

            // print PDA statistics in various colors
            GUI.color = Color.green;
            GUILayout.Label("SIT-REP", GUI.skin.GetStyle("PDA"));
            GUILayout.Label("T: " + mins + ":" + secs.ToString("00") + "\n", GUI.skin.GetStyle("PDA"));

            GUI.color = Color.white;
            GUILayout.Label("Civilians:\n" + numCivilians + " (" + percentCivilians + "%)", GUI.skin.GetStyle("PDA"));

            GUI.color = Color.blue;
            GUILayout.Label("Cops:\n" + numCops + " (" + percentCops + "%)", GUI.skin.GetStyle("PDA"));

            GUI.color = Color.yellow;
            GUILayout.Label("Infected:\n" + numInfected + " (" + percentInfected + "%)", GUI.skin.GetStyle("PDA"));

            GUI.color = Color.green;
            GUILayout.Label("Zombies:\n" + numZombies + " (" + percentZombies + "%)", GUI.skin.GetStyle("PDA"));

            GUI.color = Color.red;
            GUILayout.Label("Dead:\n" + numDead + " (" + percentDead + "%)", GUI.skin.GetStyle("PDA"));

            GUI.color = Color.magenta;
            GUILayout.Label("Escaped:\n" + numEscaped + " (" + percentEscaped + "%)", GUI.skin.GetStyle("PDA"));
        }

        // end area
        GUILayout.EndArea();
    }

    //! Function that begins Army patrols.
    void BeginArmyPatrols()
    {
        // find soldiers
        ArrayList soldiers = new ArrayList();

        foreach (Transform t in unitRoot.transform)
        {
            if (t.name.StartsWith("Soldier"))
                soldiers.Add(t);
        }

        // 6 soldiers are sent in total
        // 2 soldiers remain at the bridge
        // there are 2 patrol roots on the map
        // 2 soldiers are sent along each patrol path

        // are there more than 2 soldiers remaining?
        if (soldiers.Count > 2)
        {
            // are there more than 4 soldier's remaining?
            if (soldiers.Count > 4)
            {
                // send patrol 2

                // find the first node of the second patrol path
                GameObject pathNode4 = GameObject.Find("ArmyPatrolPathNode4");

                // found path node?
                if (pathNode4)
                {
                    // get the third soldier in the array
                    Transform t = soldiers[2] as Transform;

                    // null safety check
                    if (t)
                    {
                        // get the UnitAI
                        UnitAI unitAI = (UnitAI)t.GetComponent("UnitAI");

                        if (unitAI)
                        {
                            // set the path on the soldier's UnitAI
                            unitAI.pathNode = pathNode4;
                            unitAI.pathMode = UnitAI.ePathMode.Loop;
                            unitAI.pathDir = -1;
                            unitAI.stickyPath = true;
                        }
                    }

                    // get the third soldier in the array
                    t = soldiers[3] as Transform;

                    // null safety check
                    if (t)
                    {
                        // get the UnitAI
                        UnitAI unitAI = (UnitAI)t.GetComponent("UnitAI");

                        if (unitAI)
                        {
                            // set the path on the soldier's UnitAI
                            unitAI.pathNode = pathNode4;
                            unitAI.pathMode = UnitAI.ePathMode.Loop;
                            unitAI.pathDir = -1;
                            unitAI.stickyPath = true;
                        }
                    }
                }
            }

            // send patrol 1

            // find the first node of the first patrol path
            GameObject pathNode1 = GameObject.Find("ArmyPatrolPathNode1");

            // found path node?
            if (pathNode1)
            {
                // get the first soldier in the array
                Transform t = soldiers[0] as Transform;

                // null safety check
                if (t)
                {
                    // get the UnitAI
                    UnitAI unitAI = (UnitAI)t.GetComponent("UnitAI");

                    // set the path on the soldier's UnitAI
                    if (unitAI)
                    {
                        unitAI.pathNode = pathNode1;
                        unitAI.pathMode = UnitAI.ePathMode.Loop;
                        unitAI.pathDir = 1;
                        unitAI.stickyPath = true;
                    }
                }

                // get the second soldier in the array
                t = soldiers[1] as Transform;

                // found path node?
                if (t)
                {
                    // get the UnitAI
                    UnitAI unitAI = (UnitAI)t.GetComponent("UnitAI");

                    // set the path on the soldier's UnitAI
                    if (unitAI)
                    {
                        unitAI.pathNode = pathNode1;
                        unitAI.pathMode = UnitAI.ePathMode.Loop;
                        unitAI.pathDir = 1;
                        unitAI.stickyPath = true;
                    }
                }
            }
        }
    }

    //! Trigger callback for the player escaping trigger, located on the bridge near the civilian escape trigger.
    //! @param Collider other  The collider that entered this trigger.
    void OnTriggerEnter(Collider other)
    {
        // this trigger is only interested in the player
        if (other.gameObject.tag != "Player")
            return;

        // set the playerEscaped flag
        playerEscaped = true;

        // destroy the player object
        Destroy(other.gameObject);
    }

    //! Internal function to update current PDA message.
    //! @param ePDAMessageID messageId  The new PDA message.
    void ShowPdaMessage(ePDAMessageID messageId)
    {
        // set the PDA message id
        pdaMessage = messageId;
        // reset PDA display timer 
        pdaMessageTimeElapsed = 0;

        // play new PDA message sound (beeps)
        if (GetComponent<AudioSource>() && pdaSound)
            GetComponent<AudioSource>().PlayOneShot(pdaSound);
    }

}
