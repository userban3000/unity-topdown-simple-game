using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable {
    
    public float startingHealth;
    protected float health;
    protected bool dead;

    public event System.Action OnDeath;
    public event System.Action UpdateHealth;

    protected virtual void Start() {
        health = startingHealth;
    }

    public virtual void SetHealth(float healthToSet) {
        health = healthToSet;
    } 

    public virtual float GetHealth() {
        return health;
    }

    public virtual void TakeHit (float damage, Vector3 hitPoint, Vector3 hitDir) {

        TakeDamage(damage);
    }

    public virtual void TakeDamage (float damage) {
        health -= damage;
        if ( UpdateHealth != null ) {
            UpdateHealth();
        }
        if ( health <= 0 && !dead ) {
            Die();
        }
    }

    [ContextMenu("Force Kill")]
    protected virtual void Die () {
        dead = true;
        if ( OnDeath != null ) {
            OnDeath();
        }
        GameObject.Destroy(gameObject);
    }

}
