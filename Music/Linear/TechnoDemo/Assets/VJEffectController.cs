using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class VJEffectController : MonoBehaviour
{
    public class Filter
    {
        public float lpf = 0.0f;
        public float bpf = 0.0f;
        public float env = 0.0f;

        public float Process(float input, float cut, float bw)
        {
            lpf += cut * bpf;
            float hpf = input - lpf - bpf * bw;
            bpf += cut * hpf;
            float a = bpf * bpf;
            if (a > env)
                env = a;
            else
                env *= 0.999f;
            return env;
        }
    };

    public int NumParticles = 2000;
    public float RotationSpeed = 1.0f;
    public float LightRayAnim1 = 1.0f;
    public float LightRayAnim2 = 1.0f;
    public float LightRayAnim3 = 1.0f;
    public float LightRayAnim4 = 1.0f;
    public float LightRayAmpScale = 0.9f;
    public float LightRayDirScale = 0.5f;
    public float TeeVeeAnim1 = 1.0f;
    public float TeeVeeAnim2 = 1.0f;
    public float TeeVeeAnim3 = 1.0f;
    public float TeeVeeAnim4 = 1.0f;

    public float LowCut = 0.01f;
    public float MidCut = 0.1f;
    public float HighCut = 0.4f;

    public float LowBW = 0.01f;
    public float MidBW = 0.01f;
    public float HighBW = 0.01f;

    public bool MonitorLow = false;
    public bool MonitorMid = false;
    public bool MonitorHigh = false;

    public AudioMixer mixer;

    private Filter FilterLow = new Filter();
    private Filter FilterMid = new Filter();
    private Filter FilterHigh = new Filter();

    private float LowpassCutoff = 22050.0f;
    private float HighpassCutoff = 0.0f;
    private float DistortionLevel = 0.0f;

    public Shader CopyTextureShader;
    public Shader LightRaysShader;
    public Shader TeeVeeNoiseShader;

    private Material CopyTextureMaterial;
    private Material LightRaysMaterial;
    private Material TeeVeeNoiseMaterial;

    private RenderTexture TmpRenderTex1;
    private RenderTexture TmpRenderTex2;

    private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[1000];

    // Use this for initialization
    void Start()
    {
        CopyTextureMaterial = new Material(CopyTextureShader); CopyTextureMaterial.hideFlags = HideFlags.HideAndDontSave;
        LightRaysMaterial = new Material(LightRaysShader); LightRaysMaterial.hideFlags = HideFlags.HideAndDontSave;
        TeeVeeNoiseMaterial = new Material(TeeVeeNoiseShader); TeeVeeNoiseMaterial.hideFlags = HideFlags.HideAndDontSave;
        TmpRenderTex1 = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32); TmpRenderTex1.Create();
        TmpRenderTex2 = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32); TmpRenderTex2.Create();
    }

    void FlashyFlash(ParticleSystem p, float br, int nm)
    {
        int np = p.GetParticles(particles);
        int offs = np / nm;
        for (int i = 0; i < offs; i++)
            particles[i].color = new Color(br, br, br, 1.0f);
        var rot = Quaternion.AngleAxis(360.0f / nm, new Vector3(0.0f, 0.0f, -1.0f));
        int n = 0;
        for (int k = 1; k < nm; k++)
        {
            for (int i = 0; i < offs; i++)
            {
                particles[n + offs] = particles[n];
                particles[n + offs].position = rot * particles[n].position;
                n++;
            }
        }
        p.SetParticles(particles, np);
    }

    // Update is called once per frame
    void Update()
    {
        int MinParticles = 30;
        int MaxParticles = NumParticles;
        var lpo = GameObject.Find("LowP"); var lp = lpo.GetComponent<ParticleSystem>(); if (lp.maxParticles != NumParticles) lp.maxParticles = NumParticles;
        var mpo = GameObject.Find("MidP"); var mp = mpo.GetComponent<ParticleSystem>(); if (mp.maxParticles != NumParticles) mp.maxParticles = NumParticles;
        var hpo = GameObject.Find("HighP"); var hp = hpo.GetComponent<ParticleSystem>(); if (hp.maxParticles != NumParticles) hp.maxParticles = NumParticles;
        lp.emissionRate = FilterLow.env * (MaxParticles - MinParticles) + MinParticles;
        mp.emissionRate = FilterMid.env * (MaxParticles - MinParticles) + MinParticles;
        hp.emissionRate = FilterHigh.env * (MaxParticles - MinParticles) + MinParticles;
        lpo.transform.rotation *= Quaternion.AngleAxis(FilterLow.env * 2.0f * RotationSpeed, Vector3.forward);
        mpo.transform.rotation *= Quaternion.AngleAxis(-FilterMid.env * 5.0f * RotationSpeed, Vector3.forward);
        hpo.transform.rotation *= Quaternion.AngleAxis(FilterHigh.env * 7.0f * RotationSpeed, Vector3.forward);
        FlashyFlash(lp, FilterLow.env, 3);
        FlashyFlash(mp, FilterMid.env, 5);
        FlashyFlash(hp, FilterHigh.env, 7);
    }

    void OnAudioFilterRead(float[] data, int numchannels)
    {
        bool monitoring = MonitorLow || MonitorMid || MonitorHigh;
        for (int n = 0; n < data.Length; n += numchannels)
        {
            FilterLow.Process(data[n], LowCut, LowBW);
            FilterMid.Process(data[n], MidCut, MidBW);
            FilterHigh.Process(data[n], HighCut, HighBW);
            if (monitoring)
            {
                float m = 0.0f;
                if (MonitorLow) m += FilterLow.bpf;
                if (MonitorMid) m += FilterMid.bpf;
                if (MonitorHigh) m += FilterHigh.bpf;
                for (int i = 0; i < numchannels; i++)
                    data[n + i] = m;
            }
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        LightRaysMaterial.SetVector("_LightRayParams", new Vector4(
                0.5f + 0.2f * Mathf.Sin(Time.time * 0.51f + FilterLow.env * LightRayAnim1),
                0.5f + 0.2f * Mathf.Sin(Time.time * 0.31f + FilterLow.env * LightRayAnim2),
                0.2f + 0.1f * Mathf.Sin(Time.time * 0.11f) + FilterMid.env * LightRayAnim3,
                0.2f + 0.1f * Mathf.Sin(Time.time * 0.05f) + FilterHigh.env * LightRayAnim4));
        LightRaysMaterial.SetFloat("_AmpScale", LightRayAmpScale);
        LightRaysMaterial.SetFloat("_DirScale", LightRayDirScale);
        Graphics.Blit(src, TmpRenderTex1, LightRaysMaterial);

        TeeVeeNoiseMaterial.SetVector("_Distortion", new Vector4(
                0.02f * TeeVeeAnim1,
                0.1f * Mathf.Sin(Time.time) * TeeVeeAnim2,
                0.6f + 0.4f * Mathf.Sin(Time.time * 0.3f) * TeeVeeAnim3,
                2.0f + 1.5f * Mathf.Sin(Time.time * 0.7f) * TeeVeeAnim4));
        Graphics.Blit(TmpRenderTex1, dst, TeeVeeNoiseMaterial);
    }
}
