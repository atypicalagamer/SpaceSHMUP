using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class Enemy : MonoBehaviour
{
    [Header("Inscribed")]
    public float speed = 10f; // The movement speed is 10m/s
    public float fireRate = 0.3f; // Seconds/shot (Unused)
    public float health = 10; // Damage needed to destroy this enemy
    public float score = 100; // Points earned for destroying this
    public float powerUpDropChance = 1f; // Chance to drop a PowerUp
    public GameObject projectilePrefab;
    public float projectileSpeed = 50;
    public float shotTime;

    protected bool calledShipDestroyed = false;
    protected BoundsCheck bndCheck;

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        shotTime = Time.time + 1f / fireRate;
    }

    // This is a Property: A method that acts like a field
    public Vector3 pos
    {
        get
        {
            return this.transform.position;
        }
        set
        {
            this.transform.position = value;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();

        if (Time.time >= shotTime && bndCheck.isOnScreen)
        {
            Fire();
        }
        
        // Check whether this Enemy has gone off the bottom of the screen
        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offDown))
        {
            Destroy(gameObject);
        }
    }

    public virtual void Move()
    {
        Vector3 tempPos = pos;
        tempPos.y -= speed * Time.deltaTime;
        pos = tempPos;
    }

    public void Fire()
    {
        if (projectilePrefab == null) return;
        shotTime = Time.time + 1f / fireRate;
        GameObject projGO = Instantiate<GameObject>(projectilePrefab, transform.position, Quaternion.identity);
        ProjectileEnemy pE = projGO.GetComponent<ProjectileEnemy>();
        if (pE != null)
        {
            pE.SetType(eWeaponType.blaster);
            pE.vel = Vector3.down * projectileSpeed;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        GameObject otherGO = coll.gameObject;
        // Check for collisions with ProjectileHero
        ProjectileHero p = otherGO.GetComponent<ProjectileHero>();
        if (p != null)
        {
            // Only damage this Enemy if it's on screen
            if (bndCheck.isOnScreen)
            {
                // Get the damage amount from the Main WEAP_DICT
                health -= Main.GET_WEAPON_DEFINITION(p.type).damageOnHit;
                if (health <= 0)
                {
                    // Tell Main that this ship was destroyed
                    if (!calledShipDestroyed)
                    {
                        calledShipDestroyed = true;
                        Main.SHIP_DESTROYED(this);
                    }
                    // Destroy this Enemy
                    Destroy(this.gameObject);
                }
            }
            // Destroy the ProjectileHero regardless
            Destroy(otherGO);
        } else
        {
            print("Enemy hit by non-ProjectileHero: " + otherGO.name);
        }
    }
}
