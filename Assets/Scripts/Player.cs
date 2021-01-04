using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PlayerController))]
[RequireComponent (typeof (GunController))]
public class Player : LivingEntity {

    public float moveSpeed = 5f;
    PlayerController controller;
    GunController gunController;
    public Camera defCam;

    public Crosshair crosshair;

    protected override void Start() {
        base.Start();
        controller = GetComponent<PlayerController> ();
        gunController = GetComponent<GunController> ();
    }

    void Update() {

        //MOVEMENT INPUT
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        //LOOK INPUT
        Ray ray = defCam.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.up*gunController.GunHeight);

        if ( ground.Raycast(ray, out float rayDist) ) {
            Vector3 pointOfIntersect = ray.GetPoint(rayDist);
            Debug.DrawLine(defCam.transform.position, pointOfIntersect, Color.green);
            controller.LookAt(pointOfIntersect);
            crosshair.transform.position = pointOfIntersect;
            crosshair.DetectTargets(ray);
        }

        //GUN INPUT
        if ( Input.GetMouseButton(0) ) {
            gunController.OnTriggerHold();
        }
        if ( Input.GetMouseButtonUp(0) ) {
            gunController.OnTriggerRelease();
        }
        if ( Input.GetKeyDown(KeyCode.R) ) {
            gunController.Reload();
        }
    }

}
