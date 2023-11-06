using System.Collections.Generic;
using Unity.Multiplayer.Tools.NetStats;
using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    [Header("Bundles")]
    [SerializeField] private SFXBundle sfxBundle;
    [SerializeField] private TracksBundle tracksBundle;

    [Space(5)]
    [Header("General")]
    [Range(1, 100)][SerializeField] private uint soundUnitsLimit = 50;

    [Space(5)]
    [Header("Fade")]
    [Range(0.01f, 1.0f)][SerializeField] private float fadeInSpeed = 0.1f;
    [Range(0.01f, 1.0f)][SerializeField] private float fadeOutSpeed = 0.1f;
    [Tooltip("Should interrupt a fade in/out and play a new track")]
    [SerializeField] private bool canInterruptFade = true;

    [Space(5)]
    [Header("Volume")]
    [Range(0.0f, 1.0f)][SerializeField] private float masterVolume = 1.0f;
    [Range(0.0f, 1.0f)][SerializeField] private float musicVolume = 1.0f;
    [Range(0.0f, 1.0f)][SerializeField] private float sfxVolume = 1.0f;


    private bool initialized = false;
    private bool canPlaySFX = false;
    private bool canPlayTracks = false;


    private bool fadingIn = false;
    private bool fadingOut = false;
    private TrackEntry? fadeTargetTrack = null;


    private AudioSource trackAudioSource = null;

    private GameObject units = null;
    private List<AudioSource> audioSources = new List<AudioSource>();

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




        if (fadingIn)
            UpdateTrackFadeIn();
        else if (fadingOut)
            UpdateTrackFadeOut();
        else
            UpdateTrackVolume();
    }
    private void SetupReferences() {

        trackAudioSource = GetComponent<AudioSource>();
        if (!ILanderUtility.Utility.Validate(trackAudioSource, "Failed to get AudioSource at " + gameObject.name, ILanderUtility.Utility.ValidationLevel.WARNING)) {
            canPlaySFX = false;
            canPlayTracks = false;
        }
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




    public bool PlaySFX(string key) {
        if (!canPlaySFX) {
            Debug.LogWarning("SoundManager can not play SFX! - PlaySFX will always fail!");
            return false;
        }

        SFXEntry? targetSFXEntry = FindSFXEntry(key);
        if (targetSFXEntry == null) {
            Debug.Log("Unable to find sfx entry associated with key " + key);
            return false;
        }

        AudioSource availableAudioSource = GetAvailableAudioSource();
        if (!availableAudioSource) {
            Debug.LogWarning("Unable to find available audio source to play sfx associated with key " + key);
            return false;
        }

        float volume = masterVolume * sfxVolume * targetSFXEntry.Value.volume;
        availableAudioSource.clip = targetSFXEntry.Value.clip;
        availableAudioSource.volume = volume;
        availableAudioSource.pitch = GetRandomizedPitch(targetSFXEntry.Value.minPitch, targetSFXEntry.Value.maxPitch);
        availableAudioSource.Play();
        return true;
    }
    public bool PlayTrack() {
        if (!canPlayTracks) {
            Debug.LogWarning("SoundManager can not play tracks! - PlayTrack will always fail!");
            return false;
        }





        return true;
    }
    public void StopTrack(bool fade = false) {
        if (!trackAudioSource.isPlaying)
            return;


        if (fadingIn)
            fadingIn = false;

        //??? The fading out part is weird otherwise, setting the fade target to null might be fine.
        if (fadingOut) { 
            fadingOut = false;
            fadeTargetTrack = null;
        }

        if (fade)
            StartFadeOut();
        else
            trackAudioSource.Stop();
    }




    public void SetMasterVolume(float value) {
        masterVolume = value;
    }
    public void SetMusicVolume(float value) {
        musicVolume = value;
    }
    public void SetSFXVolume(float value) {
        sfxVolume = value;
    }
    public float GetMasterVolume() {
        return masterVolume;
    }
    public float GetMusicVolume() {
        return musicVolume;
    }
    public float GetSFXVolume() {
        return sfxVolume;
    }


    private void UpdateTrackFadeIn() {

    }
    private void UpdateTrackFadeOut() {

    }
    private void UpdateTrackVolume() {

    }


    private void StartFadeOut() {


        

    }


    private float GetRandomizedPitch(float min, float max) {
        if (min == max)
            return max;
        if (min > max)
            (max, min) = (min, max);

        return Random.Range(min, max);
    }
    private AudioSource AddAudioSource() {
        if (audioSources.Count >= soundUnitsLimit) {
            Debug.LogWarning("Unable to add new audio source! \n Audio sources limit reached!");
            return null;
        }

        if (audioSources.Count == 0)
            units = new GameObject("Units");

        var gameObject = new GameObject("AudioSource " + audioSources.Count);
        var comp = gameObject.AddComponent<AudioSource>();
        gameObject.transform.SetParent(units.transform);
        comp.loop = false;
        comp.playOnAwake = false;
        audioSources.Add(comp);
        return comp;
    }
    private AudioSource GetAvailableAudioSource() {
        if (audioSources.Count == 0)
            return AddAudioSource();

        foreach (var entry in audioSources) {
            if (!entry.isPlaying)
                return entry;
        }

        return AddAudioSource();
    }
    private SFXEntry? FindSFXEntry(string key) {
        if (key == null)
            return null;

        if (sfxBundle.entries.Length == 0)
            return null;

        foreach(var entry in sfxBundle.entries) {
            if (entry.key == key)
                return entry;
        }

        return null;
    }
    private TrackEntry? FindTrackEntry(string key) {
        if (key == null)
            return null;

        if (tracksBundle.entries.Length == 0)
            return null;

        foreach (var entry in tracksBundle.entries) {
            if (entry.key == key)
                return entry;
        }

        return null;
    }
}
