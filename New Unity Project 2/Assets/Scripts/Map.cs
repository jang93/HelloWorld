using UnityEngine;
using System.Collections;

//! This class draws a mini map.
//! NOTE the mini map requires the render texture feature available only in Unity Pro.
public class Map : MonoBehaviour
{
    //! Enable to log debug messages
    public bool debug;

    //! Texture for the map underlay (background)
    public Texture2D mapUnderlay;
    //! Texture for the map overlay (foreground)
    public Texture2D mapOverlay;
    //! Texture for the map mask (to clip render texture)
    public Texture2D mapMask;
    //! Flag to indicate black should be used as map alpha.
    public bool mapBlackIsAlpha;

    //! Rect for render texture copy.
    Rect copyRect;
    //! Target texture for render copy.
    Texture2D mapTexture;

    //! Start function.
    void Start()
    {
        // verify there is a camera attached for this map view
        if (!GetComponent<Camera>())
        {
            // log warning
            Debug.LogWarning("Map.Start() " + name + " has no Camera attached!");
            // disable this Component
            enabled = false;
            // and return
            return;
        }

        // map mask assigned?
        if (mapMask)
        {
            if (debug)
                Debug.Log("Map.Start() " + name + " creating map texture based on map mask " + mapMask.name + " (" + mapMask.width + "x" + mapMask.height + ")");

            // create a new texture with the width and height of the mask
            mapTexture = new Texture2D(mapMask.width, mapMask.height);
        }
        // map overlay assigned?
        else if (mapOverlay)
        {
            if (debug)
                Debug.Log("Map.Start() " + name + " creating map texture based on map overlay " + mapOverlay.name + " (" + mapOverlay.width + "x" + mapOverlay.height + ")");

            // create a new texture with the width and height of the overlay
            mapTexture = new Texture2D(mapOverlay.width, mapOverlay.height);
        }
        // create default map texture
        else
        {
            if (debug)
                Debug.Log("Map.Start() " + name + " creating default map texture (128x128)");

            // default map texture at 128x128
            mapTexture = new Texture2D(128, 128);
        }

        // calculate the copy rect within the map camera based on the size of the map texture
        copyRect = new Rect((GetComponent<Camera>().pixelWidth * 0.5f) - (mapTexture.width * 0.5f), (GetComponent<Camera>().pixelHeight * 0.5f) - (mapTexture.height * 0.5f), mapTexture.width, mapTexture.height);
    }

    //! OnGUI function.  Draws the main menu.
    void OnGUI()
    {
        // map underlay assigned?
        if (mapUnderlay)
        {
            // draw the map underlay
            GUI.DrawTexture(new Rect(Screen.width - mapUnderlay.width, Screen.height - mapUnderlay.height, mapUnderlay.width, mapUnderlay.height), mapUnderlay);
        }

        // map texture safety check
        if (mapTexture)
        {
            // draw the map texture
            GUI.DrawTexture(new Rect(Screen.width - mapTexture.width, Screen.height - mapTexture.height, mapTexture.width, mapTexture.height), mapTexture);
        }

        // map overlay assigned?
        if (mapOverlay)
        {
            // draw the map overlay
            GUI.DrawTexture(new Rect(Screen.width - mapOverlay.width, Screen.height - mapOverlay.height, mapOverlay.width, mapOverlay.height), mapOverlay);
        }
    }

    //! OnPostRender function.  Callback from the render texture.
    void OnPostRender()
    {
        if (debug)
            Debug.Log("Map.OnPostRender() " + name + " copyRect = " + copyRect);

        // copy pixels from the camera's render texture into the copy rect
        mapTexture.ReadPixels(copyRect, 0, 0);

        // working color
        Color c;
        // working alpha
        float a;

        // for each pixel in the map texture...
        for (int y = 0; y < mapTexture.height; ++y)
        {
            for (int x = 0; x < mapTexture.width; ++x)
            {
                // get the map texture pixel color at x, y
                c = mapTexture.GetPixel(x, y);

                // using map black as alpha?
                if (mapBlackIsAlpha && c == Color.black)
                {
                    a = 0f;
                }
                // using mask as alpha?
                else if (mapMask)
                {
                    a = mapMask.GetPixel(x, y).a;
                }
                // use color alpha
                else
                {
                    a = c.a;
                }

                // set the alpha color
                mapTexture.SetPixel(x, y, new Color(c.r, c.g, c.b, a));
            }
        }

        // apply the modified pixels
        mapTexture.Apply();
    }

}
