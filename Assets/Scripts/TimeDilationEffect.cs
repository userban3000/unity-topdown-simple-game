using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class TimeDilationEffect : MonoBehaviour {

    PostProcessVolume vol;

    ChromaticAberration chrAbrLayer;
    Grain grainLayer;

    float origChrAbrIntens;
    float origGrainIntens;

    private void Start() {
        
        vol = FindObjectOfType<PostProcessVolume>();

        vol.profile.TryGetSettings(out chrAbrLayer);
        vol.profile.TryGetSettings(out grainLayer);

        origChrAbrIntens = chrAbrLayer.intensity.value;
        origGrainIntens = grainLayer.intensity.value;

        StartCoroutine(anim(3f));

    }

    IEnumerator anim(float speed) {

        float percent = 0;

        while ( percent < 0.3 ) {
            percent += Time.deltaTime * speed;
            float interpolation = percent * 3.5f;

            Time.timeScale = Mathf.Lerp(1f, 0.1f, interpolation);
            chrAbrLayer.intensity.value = Mathf.Lerp(origChrAbrIntens, origChrAbrIntens*8f, interpolation);
            grainLayer.intensity.value = Mathf.Lerp(origGrainIntens, origGrainIntens*8f, interpolation);
            yield return null;
        }

        while ( percent < 1 ) {
            percent += Time.deltaTime * speed;
            float interpolation = Mathf.Pow(percent, 6);

            Time.timeScale = Mathf.Lerp(0.1f, 1f, interpolation);
            chrAbrLayer.intensity.value = Mathf.Lerp(origChrAbrIntens*8f, origChrAbrIntens, interpolation);
            grainLayer.intensity.value = Mathf.Lerp(origGrainIntens*8f, origGrainIntens, interpolation);

            yield return null;
        }

    }

}
