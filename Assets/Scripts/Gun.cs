using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode { Auto, Burst, Single };
    bool triggerReleasedSinceLastShot;

    [Header("Properties")]
    public FireMode fireMode;
    public Transform[] projectileSpawn;
    public Projectiles bullet;
    public float RateOfFire; //measured in ms between shots
    public float muzzleVel;
    public float bulletDamage;
    public float damageVariance;

    [Header("Ammo")]
    public int ammoInClip;
    public int ammoRemaining;
    bool isReloading;
    public float reloadTime;
    public float maxReloadAngle;

    [Header("Burst Fire Variables")]
    public int burstCount;
    int shotsRemainingInBurst;

    [Header("Effects")]
    public Transform shell;
    public Transform shellEjection;
    MuzzleFlash muzzleFlash;
    Vector3 recoilSmoothDampVel;
    float recoilRotSmoothDampVel;
    float recoilAngle;
    public float recoilPerShot;
    public float maxRecoilAngle;
    public DamagePopup ammoPopup;
    public AudioClip shootAudio;
    public AudioClip reloadAudio;

    //EVENTS
    public event System.Action UpdateAmmo;

    private void Start() {
        muzzleFlash =  GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
        ammoRemaining = ammoInClip;
    }

    void LateUpdate() {
        //RECOIL ANIM
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVel, .1f);
        if ( !isReloading) {
            recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVel, .1f);
            transform.localEulerAngles = Vector3.left * recoilAngle;
        }

        if ( !isReloading && ammoRemaining == 0 ) {
            Reload();
        }
    }

    float nextShotTime;
    void Shoot() {
        if (Time.time > nextShotTime && ammoRemaining > 0) {

            if ( fireMode == FireMode.Burst ) {
                if ( shotsRemainingInBurst == 0 ) {
                    return;
                }
                shotsRemainingInBurst--;
            }
            else if ( fireMode == FireMode.Single ) {
                if ( !triggerReleasedSinceLastShot )
                    return;
            }

            for ( int i = 0; i < projectileSpawn.Length; i++ ) {
                if ( ammoRemaining == 0 )
                    break;
                ammoRemaining--;
                if ( ammoRemaining == 0 ) {
                    DamagePopup popupInstance = Instantiate(ammoPopup, transform.position+Vector3.up*2f, Quaternion.AngleAxis(70, Vector3.right) ) as DamagePopup;
                    popupInstance.JustStart(2f, transform.position+Vector3.up*2f);
                }
                nextShotTime = Time.time + RateOfFire / 1000;
                Projectiles newBullet = Instantiate(bullet, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectiles;
                newBullet.SetSpeed(muzzleVel); 
                newBullet.SetDamage(bulletDamage + (int)Random.Range(-damageVariance, damageVariance) );
            }
            if ( UpdateAmmo != null )
                UpdateAmmo();
            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleFlash.Activate();

            transform.localPosition -= Vector3.forward * .2f;
            recoilAngle += recoilPerShot;
            recoilAngle = Mathf.Clamp(recoilAngle, 0, maxRecoilAngle);

            AudioManager.instance.PlaySound(shootAudio, transform.position);
        }
    }

    public void Reload() {
        if ( !isReloading && ammoRemaining != ammoInClip ) {
            StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound(reloadAudio, transform.position);
        }
        
    }

    public void ReplenishAmmoForced() {
        ammoRemaining = ammoInClip;
        UpdateAmmo();
    }

    IEnumerator AnimateReload() {
        isReloading = true;
        
        float reloadSpeed = 1f/reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;

        while ( percent < 1 ) {
            percent += Time.deltaTime * reloadSpeed;

            float interpolation = (-percent*percent + percent) * 4; //parabola built as 4(-x^2 + x)

            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);

            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

            yield return null;
        }

        isReloading = false;
        ammoRemaining = ammoInClip;
        UpdateAmmo();
    }

    public void Aim(Vector3 aimPoint) {
        if ( !isReloading )
            transform.LookAt(aimPoint);
    }

    public void OnTriggerHold() {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease() {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }
}
