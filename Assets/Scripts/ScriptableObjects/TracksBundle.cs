using System;
using UnityEngine;


[Serializable]
public struct TrackEntry {
    public string key;
    public AudioClip clip;
    [Range(0.0f, 1.0f)] public float volume;
}

[CreateAssetMenu(fileName = "TracksBundle", menuName = "Data/TracksBundle", order = 10)]
public class TracksBundle : ScriptableObject {
    public TrackEntry[] entries = null;
}
