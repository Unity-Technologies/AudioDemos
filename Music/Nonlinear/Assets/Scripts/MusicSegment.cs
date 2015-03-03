using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class MusicSegment : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public AudioClip[] clips;
        public AudioMixerGroup mixerGroup;
        [Range(0.0f, 1.0f)] public float volume = 1.0f;
        [Range(-1.0f, 1.0f)] public float pan = 0.0f;
        [Range(0.0f, 1.0f)] public float reverbZoneMix = 0.0f;
        public float startTime = 0.0f;
    }

    public float bpm = 120;
    public float lengthInBeats = 16;
    public float beatsPerBar = 4;
    public float fadeInTime = 0.0f;
    public float fadeOutTime = 0.0f;
    public float startTime = 0.0f;
    public Layer[] layers;
    public MusicSegment[] transitions;
    public float[] transitionProbabilities;
    public AudioMixerSnapshot mixerSnapshot;
    public float snapshotTransitionTime = 2.0f;
    public AudioClip[] stingers;

    void Start()
    {
        if (transitions.Length == 0)
        {
            transitions = new MusicSegment[1];
            transitions[0] = this;

            transitionProbabilities = new float[1];
            transitionProbabilities[0] = 1.0f;
        }
    }
}
