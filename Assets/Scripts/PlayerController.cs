using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    
    public Rigidbody rb;
    Vector3 velocity;

    void Start() {
        
    }

    public void LookAt (Vector3 planeIntersection) {
        Debug.DrawLine(rb.transform.position, planeIntersection, Color.red);
        Vector3 heightCorrectedDir = new Vector3 (planeIntersection.x, transform.position.y, planeIntersection.z);
        transform.LookAt(heightCorrectedDir);
    }

    public void Move(Vector3 receivedVelocity) {
        velocity = receivedVelocity;
    }

    void FixedUpdate() {
        rb.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
    }
}
