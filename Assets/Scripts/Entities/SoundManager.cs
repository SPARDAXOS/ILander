using UnityEngine;

public class SoundManager : MonoBehaviour {

    [Header("Bundles")]
    [SerializeField] private SFXBundle sfxBundle;
    [SerializeField] private TracksBundle tracksBundle;

    [Space(5)]

    private bool initialized = false;
    private bool canPlaySFX = false;
    private bool canPlayTracks = false;


    private AudioSource trackAudioSource = null;

    public void Initialize() {
        if (initialized) {
            Debug.LogWarning("Attempted to initialize an already intialized entity! - " + gameObject.name);
            return;
        }

        ValidateBundles();
        SetupReferences();
    }
    public void Tick() {
        if (!initialized) {
            Debug.LogWarning("Attempted to tick an unintialized entity! - " + gameObject.name);
            return;
        }




    }
    private void SetupReferences() {




    }
    private void ValidateBundles() {
        if (sfxBundle)
            canPlaySFX = true;
        else {
            Debug.LogWarning("SoundManager is missing an SFXBundle - Playing SFX will not be possible!");
            canPlaySFX = false;
        }

        if (tracksBundle)
            canPlayTracks = true;
        else {
            Debug.LogWarning("SoundManager is missing a TracksBundle - Playing tracks will not be possible!");
            canPlayTracks = false;
        }
    }




    public bool PlaySFX() {

        return true;
    }
    public bool PlayTrack() {

        return true;
    }
    public void StopTrack() {

    }

}
