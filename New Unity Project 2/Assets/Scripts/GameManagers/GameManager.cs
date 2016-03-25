using UnityEngine;
using System.Collections;

//! GameManager base class.  This is a Singleton class ("There can be only one!") that is used to perform Game management functions.  
//! It also provides and exposes some system-wide variables that are useful to other classes, for example, certain masks for collision checks.
//! The GameManager is also responsible for the Pause Menu, which can be customized per game mode through the GUISkin.
public class GameManager : MonoBehaviour
{
    //! Internal instance reference.
    static GameManager instance;

    //! If enabled, the GameManager will log debug messages.
    public bool debug;

    //! The GUISkin to use for GUI rendering.
    public GUISkin guiSkin;

    //! Public variable for optimized access to "Ignore Raycast" layer mask.
    public LayerMask ignoreRayCastMask;
    //! Public variable for optimized access to "World" layer mask.
    public LayerMask worldMask;
    //! Public variable for optimized access to "Ground" layer mask.
    public LayerMask groundMask;
    //! Public variable for optimized access to "Item" layer mask.
    public LayerMask itemMask;

    //! Protected reference to GameObject tagged "Player".
    protected UnitPlayer player;

    //! Internal flag to indicate if the game is paused and Pause Menu should be drawn.
    bool paused;

    //! Quick access to "Units" root GameObject, for enumerating Units.
    protected GameObject unitRoot;

    //! Returns the current GameManager reference.
    static public GameManager Instance
    {
        get { return instance; }
    }

    //! Start function, which checks the static internal reference, to ensure there is only ever one GameManager object, and performs initialization.
    //! Virtual so subclasses can override.
    public virtual void Start()
    {
        // if instance is non-null, this (second) instance should be destroyed
        // this ensures there is only ever one GameManager object
        if (instance)
        {
            // destroy this GameObject
            Destroy(gameObject);
            return;
        }

        // set instance
        instance = this;

        // find the GameObject tagged "Player"
        GameObject gPlayer = GameObject.FindGameObjectWithTag("Player");

        // did this fail to find Player GameObject?
        if (!gPlayer)
        {
            // log error
            Debug.LogError("GameManager.Start() " + name + " could not find player!");
        }
        else
        {
            // get the UnitPlayer Component
            player = (UnitPlayer)gPlayer.GetComponent("UnitPlayer");

            // did this fail to get UnitPlayer Component?
            if (!player)
            {
                // log error
                Debug.LogError("GameManager.Start() " + name + " could not get PlayerUnit from " + gPlayer.name + "!");
            }
        }

        // find "unitRoot" GameObject
        unitRoot = GameObject.Find("Units");

        // did this fail to find "unitRoot" GameObject?
        if (!unitRoot)
        {
            // log error
            Debug.LogError("GameManager.Start() " + name + " could not find unitRoot Units!");
        }

        // null safety check
        if (Options.Instance)
        {
            // set AudioListener volume based on audio option
            AudioListener.volume = (Options.Instance.audioOn ? 1f : 0f);
        }
    }

    //! Update function. Virtual so subclasses can override.
    public virtual void Update()
    {
        // did the user hit the Escape key?
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // pause the game
            Paused = true;
        }
    }

    //! OnGUI function.  Virtual so subclasses can override.
    public virtual void OnGUI()
    {
        // does this have a custom GUISkin assigned in the Inspector?
        if (guiSkin)
        {
            // set the custom GUISkin
            GUI.skin = guiSkin;
        }

        // is the game paused?
        if (paused)
        {
            // show the pause Window
            GUI.Window(0, new Rect(Screen.width * 0.2f, Screen.height * 0.1f, Screen.width * 0.6f, Screen.height * 0.8f), PauseMenu, "Pause Menu");
        }
    }

    //! Pause menu window function.
    //! @param int id  The ID of the GUI.Window.
    void PauseMenu(int id)
    {
        // eat some vertical space
        GUILayout.FlexibleSpace();

        // resume game button
        if (GUILayout.Button("Resume Game"))
        {
            // unpause the game
            Paused = false;
        }

        if (Options.Instance)
        {
            // options
            if (GUILayout.Button((Options.Instance.audioOn ? Options.Instance.audioOnIcon : Options.Instance.audioOffIcon)))
            {
                // unpause the game
                Options.Instance.audioOn = !Options.Instance.audioOn;

                // set AudioListener volume based on audio option
                AudioListener.volume = (Options.Instance.audioOn ? 1f : 0f);
            }
        }

        // quit to main menu button
        if (GUILayout.Button("Quit to Main Menu"))
        {
            // return to main menu
            Application.LoadLevel("MainMenu");
        }

        // eat remaining vertical space
        GUILayout.FlexibleSpace();
    }

    //! Property to get/set game pause
    public bool Paused
    {
        get 
        { 
            // get paused
            return paused; 
        }
        set
        {
            // set paused 
            paused = value;

            // paused?
            if (paused)
            {
                // pause Unity (physics, etc)
                Time.timeScale = 0f;
                // show mouse cursor
                Cursor.visible = true;
            }
            else
            {
                // unpause Unity
                Time.timeScale = 1f;
                // hide mouse cursor
                Cursor.visible = false;
            }
        }
    }

}
