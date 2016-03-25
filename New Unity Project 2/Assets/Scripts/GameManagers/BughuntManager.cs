using UnityEngine;
using System.Collections;

//! GameManager subclass for the Bughunt game mode.  Responsible for drawing Bughunt GUI elements.
public class BughuntManager : GameManager
{
    //! Mission briefing HUD message.
    public string message = "MISSION OBJECTIVES:\n1. Investigate Colony\n2. Locate Missing Colonists\n3. Escort Colonists to Exit ";
    public string civilianMessage;
    string fullMessage;

    //! Internal variable that keeps track of which character we are printing in the HUD briefing message.
    int currentCharacter;
    //! The length of time between printing each character in the HUD briefing message.
    public float characterDelay = 0.1f;
    //! The length of time to print the next character in the HUD briefing message.
    float nextCharacter;

    //! Civilian exit trigger
    public Trigger civilianExitTrigger;
    //! Civilians to rescue
    ArrayList civilians = new ArrayList();
    //! Number of civilians
    int civilianCount;
    //! Number of dead civilians
    int civilianDeadCount;

    //! Start function.  Overrides and calls GameManager.Start().
    public override void Start()
    {
        // call base class Start() function
        base.Start();

        // find/add civilian Units to civilian ArrayList
        foreach (Transform child in unitRoot.transform)
        {
            // does the child name start with "Civilian"
            if (child.name.StartsWith("Civilian"))
            {
                // try to get a Unit Component from the Civilian
                Unit unit = (Unit)child.GetComponent("Unit");

                // did we get a Unit?
                if (unit)
                {
                    // add the Unit to the civilians ArrayList
                    civilians.Add(unit);
                }
            }
        }

        // init civilian count
        civilianCount = civilians.Count;

        // append civilian count to briefing message
        civilianMessage = " (0/" + civilianCount + ")";

        // init the full message
        fullMessage = message + " " + civilianMessage;
    }

    //! Update function.  Overrides and calls GameManager.Update().  Updates drawing of the HUD briefing message.
    public override void Update()
    {
        // call base class Update() function
        base.Update();

        // is the civilianExitTrigger specified?
        if (civilianExitTrigger)
        {
            // update civilian message
            civilianMessage = " (" + civilianExitTrigger.destroyCount + "/" + civilianCount + ")";
        }

        // update next character count down timer
        nextCharacter -= Time.deltaTime;

        // time to print the next character in the message?
        if (nextCharacter <= 0f && currentCharacter < fullMessage.Length)
        {
            // next character
            ++currentCharacter;
            // reset next character count down timer
            nextCharacter = characterDelay;
        }
        else if (currentCharacter > fullMessage.Length)
        {
            // we are just going to print the entire message now
            currentCharacter = fullMessage.Length;
        }

        // update civilians
        for (int i = 0; i < civilians.Count; )
        {
            // get the Unit from the civilians ArrayList
            Unit unit = civilians[i] as Unit;

            // unit may have been destroyed
            if (!unit)
            {
                // remove null Unit from civilian ArrayList
                civilians.RemoveAt(i);
            }
            else if (unit.Dead)
            {
                // unit is dead
                // reduce civilian count
                civilianCount--;
                // remove dead Unit from civilian ArrayList
                civilians.RemoveAt(i);
            }
            else
            {
                // next civilian in civilians ArrayList
                i++;
            }
        }
    }

    //! OnGUI function.  Overrides and calls GameManager.OnGUI().  Draws the HUD briefing message.
    public override void OnGUI()
    {
        // call base class OnGUI() function
        base.OnGUI();

        // update full message
        fullMessage = message + civilianMessage;

        // print substring of full message up to current character
        GUI.Label(new Rect(0, Screen.height - 100f, Screen.width, 100), fullMessage.Substring(0, currentCharacter), GUI.skin.GetStyle("BughuntMission"));

        // has the player won by getting all living civilians to the exit?
        bool won = (civilianExitTrigger.destroyCount == civilianCount);

        // is the game over? (player won or dead)
        if (won || (player && player.Dead))
        {
            // show mouse cursor
            Cursor.visible = true;

            // area for end game displays
            GUILayout.BeginArea(new Rect(Screen.width * 0.2f, Screen.height * 0.25f, Screen.width * 0.6f, Screen.height * 0.5f));

            // each some vertical space
            GUILayout.FlexibleSpace();

            // player won?
            if (won)
            {
                // you won  message
                GUILayout.Label("You won!", GUI.skin.GetStyle("LabelCentre"));

                // play again button
                if (GUILayout.Button("Play Again"))
                {
                    // reload this level
                    Application.LoadLevel("Bughunt");
                }
            }
            else
            {
                // you died message
                GUILayout.Label("You died!", GUI.skin.GetStyle("LabelCentre"));

                // try again button
                if (GUILayout.Button("Play Again"))
                {
                    // reload this level
                    Application.LoadLevel("Bughunt");
                }
            }

            // main menu button
            if (GUILayout.Button("Main Menu"))
            {
                // return to main menu
                Application.LoadLevel("MainMenu");
            }

            // eat remaining vertical space
            GUILayout.FlexibleSpace();

            // end the area
            GUILayout.EndArea();
        }
    }

}
