using UnityEngine;
using System.Collections;

//! The Options class is a Singleton that ensures there is only ever one instance created, and available throughout the game session.
//! A static property provides global access to the instance.
public class Options : MonoBehaviour 
{
    //! Static instance reference
    static Options instance;

    //! Audio on/off flag
    public bool audioOn= true;
    //! Audio icons
    public Texture audioOnIcon;
    public Texture audioOffIcon;

    //! Property to access instance
    static public Options Instance
    {
        get { return instance; }
    }

    //! Start() function.
    void Start() 
    {
        // Singleton instance check, to ensure there is only one Options
        if (instance != null)
        {
            // destroy this GameObject
            Destroy(gameObject);
            // and return
            return;
        }

        // set the instance reference
        instance = this;

        // make sure this GameObject remains across level loads
        DontDestroyOnLoad(this);
	}
    
}
