using UnityEngine;

/// <summary>
/// Gestor central de música y efectos. Persiste entre escenas.
/// Los clips se asignan desde el Inspector (la herramienta de menú los cablea
/// automáticamente desde Assets/Sounds). Si falta alguno, simplemente no suena.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Música (loop)")]
    public AudioClip menuMusic;
    public AudioClip levelMusic;
    public AudioClip bossMusic;

    [Header("Efectos")]
    public AudioClip sfxSword;
    public AudioClip sfxFire;
    public AudioClip sfxIce;
    public AudioClip sfxLightning;
    public AudioClip sfxParry;

    [Header("Volúmenes")]
    [Range(0, 1)] public float musicVolume = 0.5f;
    [Range(0, 1)] public float sfxVolume = 0.9f;

    private AudioSource _music;
    private AudioSource _sfx;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<AudioManager>() != null) return;
        var go = new GameObject("AudioManager");
        go.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _music = gameObject.AddComponent<AudioSource>();
        _music.loop = true;
        _music.playOnAwake = false;
        _music.volume = musicVolume;

        _sfx = gameObject.AddComponent<AudioSource>();
        _sfx.playOnAwake = false;
        _sfx.volume = sfxVolume;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || _music == null) return;
        if (_music.clip == clip && _music.isPlaying) return;
        _music.clip = clip;
        _music.volume = musicVolume;
        _music.Play();
    }

    public void StopMusic() { if (_music != null) _music.Stop(); }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || _sfx == null) return;
        _sfx.PlayOneShot(clip, sfxVolume);
    }

    // Atajos cómodos para los scripts de gameplay.
    public void Sword()     => PlaySfx(sfxSword);
    public void Fire()      => PlaySfx(sfxFire);
    public void Ice()       => PlaySfx(sfxIce);
    public void Lightning() => PlaySfx(sfxLightning);
    public void Parry()     => PlaySfx(sfxParry);
}
