using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectiles : MonoBehaviour
{
    public LayerMask collisionMask;
    float speed = 10f;
    float damage = 1f;

    public Color trailColor;

    float lifetime = 3f;
    float skinWidth = .1f;

    void Start() {
        Destroy(gameObject, lifetime);

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if ( initialCollisions.Length > 0 )
            OnHit(initialCollisions[0], transform.position);

        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColor);
    }

    public void SetSpeed ( float newSpeed ) {
        speed = newSpeed;
    }

    public void SetDamage (float newDamage ) {
        damage = newDamage;
    }

    void Update() {
        float distanceToMove = speed * Time.deltaTime;
        CheckCollisions(distanceToMove);
        transform.Translate ( Vector3.forward * distanceToMove );
    }

    void CheckCollisions ( float dist ) {
        Ray ray = new Ray (transform.position, transform.forward);
        RaycastHit hit;

        if ( Physics.Raycast(ray, out hit, dist + skinWidth, collisionMask, QueryTriggerInteraction.Collide ) ) {
            OnHit(hit.collider, hit.point);
        }
    }

    void OnHit (Collider c, Vector3 hitPoint) {
        IDamageable damageableObject = c.GetComponent<IDamageable> ();
        if ( damageableObject != null ) {
            damageableObject.TakeHit(damage, hitPoint, transform.forward);
        }
        GameObject.Destroy (gameObject);
    }
}
