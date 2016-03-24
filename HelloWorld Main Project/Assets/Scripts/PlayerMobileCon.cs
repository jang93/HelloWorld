using UnityEngine;
using System.Collections;
using CnControls;

public class PlayerMobileCon : MonoBehaviour
{
    Rigidbody2D myBody;
    public float moveForce, forceStaffmul=5, facingAngleAdjustment = 0.0f;

    void Start()
    {
        myBody = this.GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        Vector2 moveVec = new Vector2(CnInputManager.GetAxis("Horizontal"), CnInputManager.GetAxis("Vertical"))*moveForce ;
        myBody.AddForce(moveVec);


        

        //rotation
        float angle = Mathf.Atan2(-moveVec.x, moveVec.y) * Mathf.Rad2Deg + facingAngleAdjustment;
        float speed = Mathf.Abs(moveVec.x) + Mathf.Abs(moveVec.y);
        if (speed > 0.0f)
        {
            //rotate by angle around the z axis.
            this.transform.rotation = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1));
        }

    }
}
