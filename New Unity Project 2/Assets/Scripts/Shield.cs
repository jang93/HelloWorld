using UnityEngine;
using System.Collections;

//! A Shield class, which provides protection from attack damage.
public class Shield : MonoBehaviour 
{
    //! The initial/maximum shield health.
    public float maxShieldHealth = 100f;
    //! The current shield health.
    float shieldHealth;

    //! The shield's recharge rate (units/second).
    public float shieldRechargeRate = 10f;
    //! The delay following a hit before the shield starts recharging.
    public float shieldRechargeDelay = 5f;
    //! Internal variable to track current recharge delay.
    float shieldRechargeWait;

    //! Icon texture to use for the Shield HUD display.
    public Texture shieldIcon;
    //! Foreground texture to use for the Shield HUD display.
    public Texture shieldBarTop;
    //! Background texture to use for the Shield HUD display.
    public Texture shieldBarBottom;

    //! Start() function
	void Start () 
    {
        // initialize current shield strength to full
        shieldHealth = maxShieldHealth;
	}
	
    //! Update() function.
	void Update () 
    {
        // decrement current shield recharge delay
        shieldRechargeWait -= Time.deltaTime;

        // is the shield ready to recharge?  does it need to recharge?
        if (shieldRechargeWait <= 0d && shieldHealth < maxShieldHealth)
        {
            // recharge the shield based on recharge rate
            shieldHealth += Time.deltaTime * shieldRechargeRate;
        }
	}

    //! Damage() function.
    //! @return float The remaining damage after removed from Shield (zero if absorbed completely). 
    //! @param float damage The damage to apply to the Shield.
    public float Damage(float damage)
    {
        // local variable to maintain residual damage
        float remainingDamage = 0f;

        // is the shield strong enough to eat all the damage?
        if (shieldHealth > damage)
        {
            // reduce shield by damage
            shieldHealth -= damage;
        }
        else
        {
            // deduct shield from damage, retaining residual
            remainingDamage = damage - shieldHealth;
            // zero shield
            shieldHealth = 0f;
        }

        // reset shield recharge wait
        shieldRechargeWait = shieldRechargeDelay;

        // return any residual damage
        return remainingDamage;
    }

    //! Property to return current shield health.
    public float ShieldHealth
    {
        get { return shieldHealth; }
    }
}
