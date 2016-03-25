using UnityEngine;
using System.Collections;

//! Class to draw the main menu GUI
public class GuiMainMenu : MonoBehaviour
{
    //! The GUISkin to use for the main menu
    public GUISkin guiSkin;
    //! Main menu background texture
    public Texture background;

    //! The control the mouse is currently hovering over (for hover description text)
    int hoverIndex = 0;

    //! The Survival button screen rect (assumes web player 600x450)
    public Rect survivalButtonRect = new Rect(32f, 112f, 256f, 78f);
    //! The Bughunt button screen rect (assumes web player 600x450)
    public Rect bughuntButtonRect = new Rect(32f, 208f, 256f, 78f);
    //! The Simulation button screen rect (assumes web player 600x450)
    public Rect simulationButtonRect = new Rect(32f, 304f, 256f, 78f);

    //! Game mode button hover descriptions
    public string[] hoverText = {
        "Select a game mode on the left.  Mouse over for description.\n\nAll these games were created with the Top Down Shooter Kit for Unity!\n\nThe Top Down Shooter Kit contains all the C# scripts you need to create custom top down shooters like these!  Source code is clear, well commented, and full of best pratices learned from years of game and Unity development experience!\n\nGet Unity and the Top Down Shooter Kit, and start making games you want to play!",
        "Defend your crashed spacecraft from endless hordes of increasingly difficult enemies on a hostile planet!",
        "Lead a squad of space marines on a search and rescue mission through a mining colony overrun with alien creatures!", 
        "Fight for you life and guide civilians to escape, as a zombie infection spreads through the city!"
    };

    //! Game mode screenshots
    public Texture[] screenshots;

    //! Screen rect for footer text
    Rect footerRect = new Rect(100f, 390f, 400f, 20f);
    //! Version number (major)
    public int majorVersion = 1;
    //! Version number (minor)
    public int minorVersion = 0;

    //! Screen rect for PowerUp URL
    Rect urlRect = new Rect(100f, 410f, 400f, 20f);

    // Start function.
    // @return IEnumerator  This function returns an IEnumerator so it can run as a CoRoutine.
    IEnumerator Start()
    {
        // check that the custom GUISkin is assigned
        if (!guiSkin)
        {
            Debug.LogError("GuiMainMenu.Start() " + name + " has no GUISkin assigned!");
        }

        // these lines ensure things are cool after returning from gameplay
        // make sure the mouse is visible
        Cursor.visible = true;
        // restore time scale
        Time.timeScale = 1f;

        // wait for Options to be initialized, if necessary
        while (!Options.Instance)
        {
            yield return new WaitForEndOfFrame();
        }

        // set AudioListener volume based on audio option
        AudioListener.volume = (Options.Instance.audioOn ? 1f : 0f);
    }

    // Update function.  Updates hover control index.
    void Update()
    {
        // get the mouse position
        Vector3 mousePosition = Input.mousePosition;
        // convert to screen coords
        mousePosition.y = Screen.height - mousePosition.y;

        // update current hover control
        if (survivalButtonRect.Contains(mousePosition))
        {
            hoverIndex = 1;
        }
        else if (bughuntButtonRect.Contains(mousePosition))
        {
            hoverIndex = 2;
        }
        else if (simulationButtonRect.Contains(mousePosition))
        {
            hoverIndex = 3;
        }
        else
        {
            hoverIndex = 0;
        }
    }

    // OnGUI function.  Draws the main menu.
    void OnGUI()
    {
        // custom GUISkin assigned?
        if (guiSkin)
        {
            // assign custom GUISkin
            GUI.skin = guiSkin;
        }

        // background texture assigned?
        if (background)
        {
            // draw the background texture full screen
            GUI.DrawTexture(new Rect(0f,0f,Screen.width,Screen.height), background);
        }

        // survival button
        if (GUI.Button(survivalButtonRect, "", GUI.skin.GetStyle("SurvivalButton")))
        {
            // load survival game scene
            Application.LoadLevel("Survival");
        }

        // bughunt button
        if (GUI.Button(bughuntButtonRect, "", GUI.skin.GetStyle("BughuntButton")))
        {
            // load bughunt game scene
            Application.LoadLevel("Bughunt");
        }

        // zombie sim button
        if (GUI.Button(simulationButtonRect, "", GUI.skin.GetStyle("SimulationButton")))
        {
            // load zombie sim game scene
            Application.LoadLevel("Simulation");
        }

        // set GUI color
        GUI.color = Color.black;
        // define area
        GUILayout.BeginArea(new Rect(316f, 112f, 256f, 384f));
        // text for current game, based on hover
        GUILayout.Label(hoverText[hoverIndex]);

        // set GUI color
        GUI.color = Color.white;
        // screenshots
        if (hoverIndex > 0)
        {
            // make sure we have the screenshot assigned
            if (hoverIndex <= screenshots.Length)
            {
                // draw screenshot
                GUILayout.Box(screenshots[hoverIndex - 1]);
            }
            else
            {
                // log warning that we don't have a screenshot
                Debug.LogWarning("GuiMainMenu.OnGUI() " + name + " does not have screenshot " + (hoverIndex - 1));
            }
        }

        // end area
        GUILayout.EndArea();

        // footer label
        GUI.Label(footerRect, "Top-Down Shooter Kit for Unity, v" + majorVersion + "." + minorVersion, GUI.skin.GetStyle("LabelCentre"));

        // set GUI color
        GUI.color = Color.blue;
        // blue text button for URL
        if (GUI.Button(urlRect, "Join us on Facebook", GUI.skin.GetStyle("URL")))
        {
            // open URL
            Application.OpenURL("http://www.facebook.com/pages/Top-Down-Shooter-Kit-for-Unity/257324277619610");
        }

        // audio button
        if (Options.Instance.audioOnIcon && Options.Instance.audioOffIcon)
        {
            // set GUI color
            GUI.color = Color.white;

            // audio toggle button
            if (GUI.Button(new Rect(Screen.width - Options.Instance.audioOnIcon.width - 16, Screen.height - Options.Instance.audioOnIcon.height - 16, Options.Instance.audioOnIcon.width, Options.Instance.audioOnIcon.height), (Options.Instance.audioOn ? Options.Instance.audioOnIcon : Options.Instance.audioOffIcon)))
            {
                // set audio option
                Options.Instance.audioOn = !Options.Instance.audioOn;

                // set AudioListener volume based on audio option
                AudioListener.volume = (Options.Instance.audioOn ? 1f : 0f);
            }
        }
    }

}

