using UnityEngine;
using System.Collections;

public class Spells : MonoBehaviour
{
    public static bool attacking; //used to stop attacking character from moving
    bool callOnce;

    Vector2 lookDirection; //calculates vector pointing at direction character is facing

    //fireball
    public Rigidbody2D fireBallPrefab; //fireball prefab
    public Transform firePoint; //used as spawnpoint for fireball
    public float fireBallSpeed; //controls speed of fireball
    float fireBallTimer; //fireball timer used to create cooldown
    public float fireBallCooldown = 2.0f; //cooldown time for fireball

    //forcepush
    public GameObject forcePushPrefab; //explosion prefab
    public Transform forcePushPoint; //used as spawnpoint for explosion
    float forcePushTimer; //used to create cooldown for forcepush
    public float forcePushCooldown = 2.0f; //cooldown time for forcepush

    //blink
    float blinkTimer; //used to create cooldown for blink
    public float blinkCooldown = 2.0f; //cooldown time for blink
    public float blinkLength;

    private Animator animator; //controls animation of character
    private Rigidbody2D charac; //character rigidbody2D
    

    // Use this for initialization
    void Start()
    {
        this.charac = this.GetComponent<Rigidbody2D>();
        this.animator = this.GetComponent<Animator>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //calculation of vector pointing at where character is facing
        var firePointpos = new Vector2(firePoint.position.x, firePoint.position.y);
        var characpos = new Vector2(charac.position.x, charac.position.y);
        lookDirection = firePointpos - characpos;

        //calculating time, resets when spell is casted
        forcePushTimer += Time.deltaTime;
        fireBallTimer += Time.deltaTime;
        blinkTimer += Time.deltaTime;

        attacking = false;
        if (Input.GetButtonDown("Fire1"))
        {
            if (fireBallTimer < fireBallCooldown)
            {
                return;
            }
            else
            {
                fireBallTimer = 0;
                this.animator.SetBool("Attack", true);
                attacking = true;
                Invoke("Fire", .3f);
            }
        }
        else if (Input.GetButtonDown("Fire2"))
        {
            if (forcePushTimer < forcePushCooldown)
            {
                return;
            }
            else
            {
                forcePushTimer = 0;
                attacking = true;
                ForcePushAnim();               
            }
        }
        else if (Input.GetButtonDown("Fire3"))
        {
            if (blinkTimer < blinkCooldown)
            {
                return;
            }
            else
            {
                blinkTimer = 0;
                attacking = true;
                Blink();
            }
        }
    }



    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Enemy"&& !callOnce)
        {
            col.rigidbody.isKinematic = true;
            callOnce = true;
        }      
    }


    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.tag == "Enemy")
        {
            col.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
            callOnce = false;
        }

    }


    void Fire()
    {
        
        Rigidbody2D fireBall = Instantiate(fireBallPrefab, firePoint.position, firePoint.rotation) as Rigidbody2D;    
        fireBall.velocity = lookDirection*fireBallSpeed;
        Physics2D.IgnoreCollision(fireBall.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        this.animator.SetBool("Attack", false);
    
    }
    void ForcePushAnim()
    {
        this.animator.SetBool("Forcepush", true);
        Invoke("ForcePush", 1.0f);
    }
    void ForcePush()
    {
        this.animator.SetBool("Forcepush", false);
        GameObject forcePush = Instantiate(forcePushPrefab, forcePushPoint.position, forcePushPoint.rotation) as GameObject;
        Physics2D.IgnoreCollision(forcePush.GetComponent<Collider2D>(), GetComponent<Collider2D>());
     
    }
    void Blink()
    {
        this.charac.MovePosition(this.charac.position+lookDirection*blinkLength);
     
    }


}
