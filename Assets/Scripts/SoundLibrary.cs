using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour {
    
    public SoundGroup[] soundGroups;

    Dictionary<string, AudioClip[]> groupDict = new Dictionary<string, AudioClip[]>();

    private void Awake() {
        foreach ( SoundGroup soundGroup in soundGroups ) {
            groupDict.Add(soundGroup.groupID, soundGroup.group);
        }
    }

    public AudioClip GetClipFromName ( string name ) {
        if ( groupDict.ContainsKey(name) ) {
            AudioClip[] sounds = groupDict[name];
            return sounds[Random.Range(0,sounds.Length)];
        } else {
            return null;
        }
    }

    [System.Serializable]
    public class SoundGroup {
        public string groupID;
        public AudioClip[] group;
    }

}
