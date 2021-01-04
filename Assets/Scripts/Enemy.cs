using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class Enemy : LivingEntity {
    
    public enum State { Idle, Chasing, Attacking }; //states enemy can be in
    State currentState;

    NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetLE;

    float attackDistanceTreshold = .5f;
    float timeBetweenAttacks = 1;
    public float damage;
    public float damageVariance;
    public float speed;

    float nextAttackTime;

    float enemyCollisionRadius;
    float targetCollisionRadius;

    Material skinMaterial;
    public Color skinColor;
    Color originalColour;

    bool hasTarget;

    public ParticleSystem deathEffect;
    public ParticleSystem hitEffect;

    public DamagePopup dmgPopup;

    public event System.Action<Vector3, Color> DiedAt;

    private void Awake() {
        pathfinder = GetComponent<NavMeshAgent> ();

        pathfinder.speed = speed;
        skinMaterial = GetComponent<Renderer> ().material;
        skinMaterial.color = skinColor;
        originalColour = skinColor;

        if ( GameObject.FindGameObjectWithTag ("Player") != null )  {
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag ("Player").transform;
            targetLE = target.GetComponent<LivingEntity> ();

            enemyCollisionRadius = GetComponent<CapsuleCollider> ().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider> ().radius;
        }

    }

    protected override void Start() {
        base.Start();

        if ( hasTarget )  {
            currentState = State.Chasing;

            targetLE.OnDeath += onTargetDeath;

            StartCoroutine (UpdatePathfinding());
        }
    }

    public void SetCharacteristics(float moveSpeed, int newDamage, int newDamageVariance, float enemyHealth, Color enemyColor) {
        pathfinder.speed = moveSpeed;

        damage = newDamage;
        damageVariance = newDamageVariance;
        startingHealth = enemyHealth;

        skinMaterial = GetComponent<Renderer> ().material;
        skinMaterial.color = enemyColor;
        originalColour = skinMaterial.color;
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDir) {

        AudioManager.instance.PlaySound("Impact", transform.position);

        if ( damage >= health ) { //if it died, make big particle go whoosh
            ParticleSystem instDeathFX = Instantiate(deathEffect, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDir)) as ParticleSystem;
            Material mat = instDeathFX.GetComponent<Renderer>().material;
            mat.color = originalColour;
            AudioManager.instance.PlaySound("Enemy Death", transform.position);
            Destroy(instDeathFX, 3f);
            DiedAt(transform.position, originalColour);

        } else { //if took damage but didnt die, make smol particle go wosh
            ParticleSystem instHitFx = Instantiate(hitEffect, hitPoint, Quaternion.FromToRotation(Vector3.back, hitDir) ) as ParticleSystem;
            Material mat = instHitFx.GetComponent<Renderer>().material;
            mat.color = originalColour;
            Destroy(instHitFx, 3f);
            //StartCoroutine(Stagger(.5f));
        }

        DamagePopup popupInstance = Instantiate(dmgPopup, hitPoint, Quaternion.AngleAxis(70, Vector3.right) ) as DamagePopup;
        popupInstance.Customize(2f, damage, hitPoint);

        base.TakeHit(damage, hitPoint, hitDir);
    }

    void onTargetDeath () {
        hasTarget = false;
        currentState = State.Idle;
    }

    void Update() {

        if ( hasTarget ) {
            if ( Time.time > nextAttackTime ) {
                float sqrDistToTarget = ( target.position - transform.position).sqrMagnitude;
                if ( sqrDistToTarget < Mathf.Pow(attackDistanceTreshold + enemyCollisionRadius + targetCollisionRadius,2) ) {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    StartCoroutine(Attack());
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                }
            }
        }

    }

    IEnumerator Stagger(float t) {
        float originalSpeed = pathfinder.speed;
        if ( pathfinder.enabled ) {
            pathfinder.speed *= 0.5f;
        }
        yield return new WaitForSeconds(t);
        pathfinder.speed = originalSpeed;
    }

    IEnumerator Attack() {
        currentState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 originalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (enemyCollisionRadius);

        float attackSpeed = 3f;
        float percent = 0f;

        skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;

        while ( percent <= 1 ) {

            if ( percent >= .5f && !hasAppliedDamage ) {
                hasAppliedDamage = true;
                targetLE.TakeDamage(damage + (int)Random.Range(-damageVariance, damageVariance));
            }
            
            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-percent*percent + percent) * 4; //parabola built as 4(-x^2 + x)

            transform.position = Vector3.Lerp (originalPosition, attackPosition, interpolation);

            yield return null;
        }

        currentState = State.Chasing;
        pathfinder.enabled = true;

        skinMaterial.color = originalColour;
    }

    IEnumerator UpdatePathfinding() {
        float refreshRate = 0.25f; //measured in seconds

        while ( hasTarget ) {
            if ( currentState == State.Chasing ) {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (enemyCollisionRadius + targetCollisionRadius + attackDistanceTreshold/2);
                if ( !dead )
                    pathfinder.SetDestination(targetPosition);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }

}
