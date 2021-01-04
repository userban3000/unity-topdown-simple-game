using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePopup : MonoBehaviour {
    
    GameObject popupInstance;

    float speed;
    float damage;
    Vector3 location;

    public void JustStart(float popupSpeed, Vector3 newLocation) {
        StartCoroutine(AnimatePopupClean(popupSpeed, newLocation));
    }

    public void Customize (float newSpeed, float newDamage, Vector3 newLocation) {
        speed = newSpeed;
        damage = newDamage;
        location = newLocation;
        StartCoroutine(AnimatePopup(speed, damage, location));
    }

    IEnumerator AnimatePopupClean(float popupSpeed, Vector3 location) {
        float percent = 0f;

        Vector3 destination = location + Vector3.up + Vector3.left;

        TextMesh popText = FindObjectOfType(typeof(TextMesh)) as TextMesh;
        Color color = popText.color;
        float initialSize = popText.characterSize;

        while ( percent < 1 ) {
            percent += Time.deltaTime * popupSpeed;
            
            transform.position = Vector3.Lerp(location, destination, percent);
            popText.color = Color.Lerp(color, Color.clear, percent);
            popText.characterSize = Mathf.Lerp(initialSize, initialSize/2, percent);

            yield return null;
        }

        Destroy(this.gameObject);

    }

    IEnumerator AnimatePopup(float popupSpeed, float damage, Vector3 location) {
        float percent = 0f;

        Vector3 destination = location + Vector3.up + Vector3.left;

        TextMesh popText = FindObjectOfType(typeof(TextMesh)) as TextMesh;
        popText.text = damage.ToString();
        Color color = popText.color;
        float initialSize = popText.characterSize;

        while ( percent < 1 ) {
            percent += Time.deltaTime * popupSpeed;
            
            transform.position = Vector3.Lerp(location, destination, percent);
            popText.color = Color.Lerp(color, Color.clear, percent);
            popText.characterSize = Mathf.Lerp(initialSize, initialSize/2, percent);

            yield return null;
        }

        Destroy(this.gameObject);

    }

}
