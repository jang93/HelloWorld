using UnityEngine;
using System.Collections;

public class FireBallShoot : MonoBehaviour
{

    public Rigidbody2D fireBallPrefab;
    public Transform firePoint;
    float shootTimer;
    float coolDown=2.0f;
    Vector2 lookDirection;
    
    public float speed;
    private Animator animator;
    private Rigidbody2D charac;

    // Use this for initialization
    void Start()
    {
        this.charac = this.GetComponent<Rigidbody2D>();
        this.animator = this.GetComponent<Animator>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var firePointpos = new Vector2(firePoint.position.x, firePoint.position.y);
        var characpos = new Vector2(charac.position.x, charac.position.y);
        lookDirection = firePointpos - characpos;

        shootTimer += Time.deltaTime;
        if (shootTimer < coolDown)
        {
            return;
        }
        if (Input.GetButtonDown("Fire1"))
        {
            shootTimer = 0;
            this.animator.SetBool("Attack", true);

            Invoke("Fire", .3f);
        }
    }

    void Fire()
    {
        Rigidbody2D fireBall = Instantiate(fireBallPrefab, firePoint.position, firePoint.rotation) as Rigidbody2D;
        //Rigidbody2D fireBall = Instantiate(fireBallPrefab, transform.position, Quaternion.Euler(new Vector3(0, 0, 1))) as Rigidbody2D;
    
        fireBall.velocity = lookDirection*speed;
       //fireBall.AddForce((lookDirection)*1000,ForceMode2D.Impulse);
        Physics2D.IgnoreCollision(fireBall.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        // coolDown = Time.time + attackSpeed;
        this.animator.SetBool("Attack", false);
    }

}
