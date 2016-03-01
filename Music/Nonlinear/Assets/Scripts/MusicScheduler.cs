using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class MusicScheduler : MonoBehaviour
{
    public enum StingerType
    {
        Immediate,
        NextBeat,
        NextBar,
        NextSegment,
    }

    public class DebugRect
    {
        public float x1, y1, x2, y2, t = 0.0f, alpha = 1.0f;

        public DebugRect(float x1, float y1, float x2, float y2)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }

        public void Render()
        {
            float w = 0.3f;
            float x3 = x1 + (x2 - x1 - w) * t;
            float x4 = x3 + w;
            DrawStuff.DrawQuad(new Vector3(x1, y1, 0), new Vector3(x2, y1, 0), new Vector3(x2, y2, 0), new Vector3(x1, y2, 0), new Color(0, 0, 0, 0.4f * alpha));
            DrawStuff.DrawQuad(new Vector3(x3, y1, 0), new Vector3(x4, y1, 0), new Vector3(x4, y2, 0), new Vector3(x3, y2, 0), Color.red);
            DrawStuff.DrawFrame(new Vector3(x1, y1, 0), new Vector3(x2, y1, 0), new Vector3(x2, y2, 0), new Vector3(x1, y2, 0), Color.white);
        }
    };

    public class MusicLayer
    {
        GameObject go = new GameObject();
        GameObject[] players = new GameObject[2];
        AudioSource[] sources = new AudioSource[2];
        AudioClip currClip;
        int index;
        int currSource = 0;
        double[] sourceStartTime = new double[2];
        double[] sourceEndTime = new double[2];
        System.Random random = new System.Random();
        List<DebugRect> debugRects = new List<DebugRect>();
        public DebugRect currDebugRect, prevDebugRect;

        public MusicLayer(int index, GameObject root, string name)
        {
            this.index = index;

            go.transform.parent = root.transform;
            go.name = name;

            for (int i = 0; i < 2; i++)
            {
                var child = new GameObject();
                child.name = name + " player " + i;
                players[i] = child;
                child.transform.parent = go.transform;

                var s = child.AddComponent<AudioSource>();
                s.spatialBlend = 0.0f;
                s.dopplerLevel = 0.0f;
                s.playOnAwake = false;
                sources[i] = s;
            }
        }

        public void NextClip(MusicScheduler musicScheduler)
        {
            if (index < musicScheduler.currSegment.layers.Length)
            {
                MusicSegment.Layer layer = musicScheduler.currSegment.layers[index];
                if (layer.clips.Length > 0)
                {
                    currSource = 1 - currSource;
                    var s = sources[currSource];

                    var nextClip = layer.clips[random.Next(0, layer.clips.Length)];
                    while (nextClip == currClip)
                        nextClip = layer.clips[random.Next(0, layer.clips.Length)];

                    s.Stop();

                    float beatLength = 60.0f / musicScheduler.currSegment.bpm;
                    double t0 = musicScheduler.GetNextEventStartTime() + layer.startTime * beatLength;
                    s.outputAudioMixerGroup = layer.mixerGroup;
                    s.clip = nextClip;
                    s.volume = (musicScheduler.currSegment.fadeInTime > 0.0f) ? 0.0f : layer.volume;
                    s.panStereo = layer.pan;
                    s.reverbZoneMix = layer.reverbZoneMix;
                    s.PlayScheduled(t0);
                    sourceStartTime[currSource] = t0;
                    sourceEndTime[currSource] = t0 + nextClip.length;

                    prevDebugRect = currDebugRect;
                    float x = (float)(t0 - musicScheduler.GetStartTime()), y = index * 5.0f + currSource * 2.0f;
                    currDebugRect = new DebugRect(x, y, x + nextClip.length, y + 2.0f);
                    debugRects.Add(currDebugRect);

                    DrawStuff.AddTextMesh(go, x, y, nextClip.name);

                    musicScheduler.camTarget = Mathf.Max(musicScheduler.camTarget, (float)(musicScheduler.GetNextEventStartTime() - musicScheduler.GetStartTime() + nextClip.length * 0.5));
                }
            }
        }

        public void Update(MusicScheduler musicScheduler, double currTime)
        {
            if (musicScheduler.currSegment != null && index < musicScheduler.currSegment.layers.Length)
            {
                MusicSegment.Layer layer = musicScheduler.currSegment.layers[index];
                if (layer != null)
                {
                    float beatLength = 60.0f / musicScheduler.currSegment.bpm;
                    for (int i = 0; i < 2; i++)
                    {
                        if (sources[i] == null || !sources[i].isPlaying)
                            continue;
                        float fade = 1.0f;
                        if (musicScheduler.currSegment.fadeInTime > 0.0f)
                        {
                            float deltaTime = (float)(currTime - sourceStartTime[i]);
                            fade *= Mathf.Clamp(deltaTime / (musicScheduler.currSegment.fadeInTime * beatLength), 0.0f, 1.0f);
                        }
                        if (musicScheduler.currSegment.fadeOutTime > 0.0f)
                        {
                            float deltaTime = (float)(sourceEndTime[i] - currTime);
                            fade *= Mathf.Clamp(deltaTime / (musicScheduler.currSegment.fadeOutTime * beatLength), 0.0f, 1.0f);
                        }
                        sources[i].volume = layer.volume * fade;
                    }
                }
            }
        }

        public void DrawDebug()
        {
            if (prevDebugRect != null)
            {
                prevDebugRect.t = Mathf.Max(prevDebugRect.t, sources[1 - currSource].time / sources[1 - currSource].clip.length);
                prevDebugRect.alpha = sources[1 - currSource].volume;
            }
            if (currDebugRect != null)
            {
                currDebugRect.t = Mathf.Max(currDebugRect.t, sources[currSource].time / sources[currSource].clip.length);
                currDebugRect.alpha = sources[currSource].volume;
            }
            foreach (var r in debugRects)
                r.Render();
        }
    };

    public class ScheduledStinger
    {
        public AudioClip clip;
        public float level;
        public double time;
        public bool disposed;
    }

    public MusicSegment startSegment;
    public float camTarget = 0.0f;
    public float camPosition = 0.0f;
    public Camera camera;
    public AudioMixerGroup stingerMixerGroup;
    public float stingerLevel = 1.0f;

	Material mat;
    MusicLayer[] layers = new MusicLayer[16];
    System.Random random = new System.Random();
    AudioSource stingerSource;
    double prefetchTime = 1.0;
    double startTime;
    double currSegmentStartTime;
    double nextSegmentStartTime;
    MusicSegment currSegment;
    MusicSegment nextSegment;
    List<ScheduledStinger> scheduledStingers = new List<ScheduledStinger>();
    List<MusicSegment> allSegments = new List<MusicSegment>();

    public double GetNextEventStartTime()
    {
        return nextSegmentStartTime;
    }

    public double GetStartTime()
    {
        return startTime;
    }

    void Start()
    {
		mat = new Material(Shader.Find("Sprites/Default"));

        var root = new GameObject();
        root.transform.parent = gameObject.transform;
        root.name = "Music Layers";

        stingerSource = root.AddComponent<AudioSource>();
        stingerSource.spatialBlend = 0.0f;
        stingerSource.dopplerLevel = 0.0f;
        stingerSource.outputAudioMixerGroup = stingerMixerGroup;

        for (int n = 0; n < layers.Length; n++)
            layers[n] = new MusicLayer(n, root, "Layer " + (n + 1));

        currSegment = startSegment;
        nextSegment = startSegment;
        if (currSegment.mixerSnapshot != null)
            currSegment.mixerSnapshot.TransitionTo(currSegment.snapshotTransitionTime);

        FindAllSegments(startSegment);

        startTime = AudioSettings.dspTime;
        nextSegmentStartTime = startTime + 2.0; // It may take some time before we are done initializing all game objects
    }

    void FindAllSegments(MusicSegment segment)
    {
        List<MusicSegment> newSegments = new List<MusicSegment>();
        if (!allSegments.Contains(segment))
            allSegments.Add(segment);
        foreach (var s in segment.transitions)
            if (!allSegments.Contains(s))
                newSegments.Add(s);
        if (newSegments.Count > 0)
        {
            allSegments.AddRange(newSegments);
            foreach (var s in newSegments)
                FindAllSegments(s);
        }
    }

    public void TransitionTo(MusicSegment s)
    {
        nextSegment = (s != null) ? s : currSegment.transitions[random.Next(0, currSegment.transitions.Length)];

        float beatLength = 60.0f / currSegment.bpm;
        double len = currSegment.lengthInBeats * beatLength;
        nextSegmentStartTime = currSegmentStartTime + len + nextSegment.startTime * beatLength;
    }

    void Update()
    {
        double currTime = AudioSettings.dspTime;

        if (currSegment != null)
        {
            if (currTime + prefetchTime >= nextSegmentStartTime)
            {
                currSegment = nextSegment;
                nextSegment = currSegment.transitions[random.Next(0, currSegment.transitions.Length)];
                if (currSegment.mixerSnapshot != null)
                    currSegment.mixerSnapshot.TransitionTo(currSegment.snapshotTransitionTime);

                float beatLength = 60.0f / currSegment.bpm;
                double len = currSegment.lengthInBeats * beatLength;
                for (int n = 0; n < layers.Length; n++)
                {
                    layers[n].NextClip(this);
                    if (n == 0 && layers[n].currDebugRect != null)
                        DrawStuff.AddTextMesh(gameObject, layers[n].currDebugRect.x1, -3.0f, currSegment.name);
                }

                currSegmentStartTime = nextSegmentStartTime;
                nextSegmentStartTime += len + nextSegment.startTime * beatLength;
            }

            for (int n = 0; n < layers.Length; n++)
                layers[n].Update(this, currTime);
        }

        foreach (var s in scheduledStingers)
        {
            if (s.time < currTime)
            {
                stingerSource.PlayOneShot(s.clip, s.level);
                s.disposed = true;
            }
        }

        scheduledStingers.RemoveAll(item => item.disposed);

        if (camera != null)
        {
            camPosition += (camTarget - camPosition) * 0.02f;
            camera.transform.position = new Vector3(camPosition, camera.transform.position.y, camera.transform.position.z);
        }
    }

    void OnRenderObject()
    {
        mat.SetPass(0);
        for (int n = 0; n < layers.Length; n++)
            layers[n].DrawDebug();
    }

    void PlayStinger(StingerType stingerType)
    {
        if (currSegment != null && currSegment.stingers.Length > 0)
        {
            var stingerClip = currSegment.stingers[random.Next(0, currSegment.stingers.Length)];

            if (stingerType == StingerType.Immediate)
            {
                stingerSource.PlayOneShot(stingerClip, stingerLevel);
                return;
            }

            var s = new ScheduledStinger();
            s.clip = stingerClip;
            s.level = stingerLevel;

            double dspTime = AudioSettings.dspTime;
            double unitLength = 60.0 / currSegment.bpm;
            switch (stingerType)
            {
                case StingerType.NextBar:
                    unitLength *= currSegment.beatsPerBar;
                    break;
                case StingerType.NextSegment:
                    unitLength *= currSegment.lengthInBeats;
                    break;
            }

            s.time = currSegmentStartTime;
            while (s.time < dspTime)
                s.time += unitLength;

            scheduledStingers.Add(s);
        }
    }

    bool showGUI = false;

    void OnGUI()
    {
        if (GUILayout.Button("GUI"))
            showGUI = !showGUI;

        if (!showGUI)
            return;

        if (GUILayout.Button("Play stinger immediately"))
        {
            PlayStinger(StingerType.Immediate);
        }

        if (GUILayout.Button("Schedule stinger on beat"))
        {
            PlayStinger(StingerType.NextBeat);
        }

        if (GUILayout.Button("Schedule stinger on bar"))
        {
            PlayStinger(StingerType.NextBar);
        }

        if (GUILayout.Button("Schedule stinger on transition"))
        {
            PlayStinger(StingerType.NextSegment);
        }

        foreach (var t in allSegments)
        {
            if (GUILayout.Button("Transition to " + t.name))
            {
                TransitionTo(t);
            }
        }
    }
}
