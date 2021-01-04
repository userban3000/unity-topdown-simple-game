using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public enum AudioChannel {Master, SFX, Music};

    float masterVolumePercent = 1;
    float sfxVolumePercent = 1;
    float musicVolumePercent = .4f;

    AudioSource[] musicSources;
    int activeMusicSourceIndex;

    public static AudioManager instance;

    Transform audioListener;
    Transform playerT;

    SoundLibrary library;

    private void Awake() {

        masterVolumePercent = PlayerPrefs.GetFloat("master vol", masterVolumePercent);
        sfxVolumePercent = PlayerPrefs.GetFloat("sfx vol", sfxVolumePercent);
        musicVolumePercent = PlayerPrefs.GetFloat("music vol", musicVolumePercent);

        if ( instance != null ) {
            Destroy(gameObject);
        } else {

            instance = this;
            DontDestroyOnLoad(gameObject);

            audioListener = FindObjectOfType<AudioListener> ().transform;
            playerT = FindObjectOfType<Player> ().transform;
            library = GetComponent<SoundLibrary> ();

            musicSources = new AudioSource[2];
            for ( int i = 0; i < 2; i++ ) {
                GameObject newMusicSource = new GameObject("Music Source" + (i+1) );
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform;
            }
        }
    }

    private void Update() {
        if ( playerT != null ) {
            audioListener.position = playerT.position;
        }
    }

    public void SetVolume( float volumePercent, AudioChannel channel ) {
        switch (channel) {
            case AudioChannel.Master:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.SFX:
                sfxVolumePercent = volumePercent;
                break;
            case AudioChannel.Music:
                musicVolumePercent = volumePercent;
                break;
        }

        musicSources[0].volume = musicVolumePercent * masterVolumePercent;
        musicSources[1].volume = musicVolumePercent * masterVolumePercent;

        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);

    }

    public void PlayMusic ( AudioClip clip, float fadeDuration) {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex; //flips between 0 and 1 pog
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play();
        StartCoroutine(AnimateMusicCrossfade(fadeDuration));
    }

    IEnumerator AnimateMusicCrossfade(float duration) {
        float percent = 0;

        while ( percent < 1 ) {
            percent += Time.deltaTime * (1/duration);
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent*masterVolumePercent, percent);
            musicSources[1-activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent*masterVolumePercent, 0, percent);
            yield return null;
        }
    }

    public void PlaySound (AudioClip clip, Vector3 pos) {
        if ( clip != null )
            AudioSource.PlayClipAtPoint(clip,pos, sfxVolumePercent*masterVolumePercent);
    }

    public void PlaySound (string soundName, Vector3 pos) {
        PlaySound(library.GetClipFromName(soundName), pos);
    }

}
