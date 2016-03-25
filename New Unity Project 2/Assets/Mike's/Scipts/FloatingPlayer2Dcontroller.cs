using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class FloatingPlayer2Dcontroller : MonoBehaviour {
    Rigidbody2D mybody;
    public float moveForce ;
    public float forceStaffmul ;
    public float facingAngleAdjustment = 0.0f;


	void Start ()
    {
        mybody = this.GetComponent<Rigidbody2D>();



    }
	
	
	void FixedUpdate ()
    {
        //movement and the force staff
        Vector2 moveVec = new Vector2(CrossPlatformInputManager.GetAxis("Horizontal"),CrossPlatformInputManager.GetAxis("Vertical"))*moveForce;
        bool isForceStaff = CrossPlatformInputManager.GetButton("forceStaff");

        mybody.AddForce(moveVec * (isForceStaff ? forceStaffmul : 1));

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
