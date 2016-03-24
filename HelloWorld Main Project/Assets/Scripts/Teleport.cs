using UnityEngine;
using System.Collections;
using CnControls;

public class Teleport : MonoBehaviour
{
    Rigidbody2D myBody;
    float teleportDistance;

    void Start()
    {
        myBody = this.GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update () {
        bool isAbility1 = CnInputManager.GetButton("ability1");
        Debug.Log(isAbility1 ? teleportDistance : 1);
        //m

    }
}
