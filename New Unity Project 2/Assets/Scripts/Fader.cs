using UnityEngine;
using System.Collections;

//! A small class that fades materials on renderers by reducing alpha.  Will also fade any Light attached to the GameObject by reducing intensity.
//! Useful for fading out effects like bullet hits and blood puddles (if desired).
public class Fader : MonoBehaviour
{
    //! How long to wait before beginning fade.
    public float waitTime = 10f;
    //! How long to fade.
    public float fadeTime = 1f;
    float elapsed;

    //! The beginning alpha value of the fade operation.
    public float startAlpha = 1f;

    //! Enable to destroy the GameObject when the fade is complete.
    public bool destroyOnFade = true;
    //! An optional object to destroy when fadingis complete (for example, a parent).
    public Transform destroyObject;

    //! The renderer[s] to apply the fade to.
    public Renderer[] renderers;
    string[] colorNames;

    //! Start function, which performs some initialization.
    void Start()
    {
        // have no renderers have been assigned in the Inspector?
        if (renderers.Length == 0)
        {
            // use any renderer on this GameObject
            if (GetComponent<Renderer>())
            {
                // allocate one renderer array
                renderers = new Renderer[1];
                // store renderer in array
                renderers[0] = GetComponent<Renderer>();
            }
        }

        // allocate new string array for each renderer
        colorNames = new string[renderers.Length];

        // iterate through each renderer
        for (int i = 0; i < renderers.Length; ++i)
        {
            // get the renderer
            Renderer r = renderers[i] as Renderer;

            // null safety check
            if (r)
            {
                // determine what color property is on this renderer's material
                if (r.material.HasProperty("_Color"))
                    colorNames[i] = "_Color";
                else if (r.material.HasProperty("_TintColor"))
                    colorNames[i] = "_TintColor";
            }
        }

        // make sure alpha is not negative
        if (startAlpha <= 0f)
        {
            startAlpha = 0f;
        }
    }

    //! Update function, where the fade is performed.
    void Update()
    {
        // are we still waiting to start this fader?
        if (waitTime > 0f)
        {
            // wait time count down
            waitTime -= Time.deltaTime;

            // still waiting?
            if (waitTime > 0f)
            {
                return;
            }
        }

        // accumulate elapsed time
        elapsed += Time.deltaTime;

        // are we done fading?
        if (elapsed >= fadeTime && destroyOnFade)
        {
            // is a specific destroy object specified?
            if (destroyObject)
            {
                // destroy the destroy object 
                Destroy(destroyObject.gameObject);
            }
            else
            {
                // destroy this GameObject
                Destroy(gameObject);
            }
        }
        else
        {
            // current alpha
            float curAlpha = 1f;

            if (elapsed < fadeTime)
            {
                // scale current alpha by elapsed / fadetime
                curAlpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeTime);
            }

            // iterate through renderers
            for (int i = 0; i < renderers.Length; ++i)
            {
                // get renderer
                Renderer r = renderers[i] as Renderer;

                // null safety check
                if (r)
                {
                    // get the color name from the color names array
                    Color color = r.material.GetColor(colorNames[i]);
                    // set the color's alpha
                    color.a = curAlpha;
                    // apply it to the material
                    r.material.SetColor(colorNames[i], color);
                }
            }

            // is there a light on this fader?
            if (GetComponent<Light>())
            {
                // modify the light intensity
                GetComponent<Light>().intensity = curAlpha;
            }

            // have we reached the maximum alpha?
            if (curAlpha >= 1f)
            {
                // disable this script
                enabled = false;
            }
        }
    }
}
