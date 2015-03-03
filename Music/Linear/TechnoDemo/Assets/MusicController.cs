using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class MusicController : MonoBehaviour
{
    public AudioSource drivingSource;
    public AudioMixerSnapshot[] snapshots;
    public int[] segmentRepeats;
    public float[] transitionTimes;

    private float drivingSourceLastTime = -1.0f;
    private int currSnapshot = 0;
    private int segmentCounter = 0;
    private bool showGUI = false;

    void Start()
    {
    }

    void Update()
    {
        if (drivingSource.time < drivingSourceLastTime)
        {
            if (++segmentCounter == segmentRepeats[currSnapshot])
            {
                segmentCounter = 0;
                currSnapshot = (currSnapshot + 1) % snapshots.Length;
                snapshots[currSnapshot].TransitionTo(transitionTimes[currSnapshot]);
            }
        }
        drivingSourceLastTime = drivingSource.time;
    }

    void OnGUI()
    {
        if (GUILayout.Button("GUI"))
            showGUI = !showGUI;

        if (showGUI)
        {
            int index = 0;
            foreach (var s in snapshots)
            {
                if (GUILayout.Button("Switch to snapshot " + s))
                {
                    segmentCounter = 0;
                    currSnapshot = index;
                    snapshots[currSnapshot].TransitionTo(transitionTimes[currSnapshot]);
                }
                index++;
            }
        }
    }
}
