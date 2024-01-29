using UnityEngine;
using UnityEngine.UI;


public class AudioManager : MonoBehaviour
{
    #region SINGLETON
    private static AudioManager _instance = null;
    public static AudioManager Instance { get { return _instance; } }
    private AudioManager() { }
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _musicAudioSource.Play();
        }
        else
        {
            Destroy(gameObject);
        }

        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume");
            _musicAudioSource.volume = MasterVolume * MusicVolume;
            _sfxAudioSource.volume = MasterVolume * SFXVolume;
        }

        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume");
            _musicAudioSource.volume = MasterVolume * MusicVolume;
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            SFXVolume = PlayerPrefs.GetFloat("SFXVolume");
            _sfxAudioSource.volume = MasterVolume * SFXVolume;
        }

        if (_masterVolumeSlider != null)
            _masterVolumeSlider.value = MasterVolume;

        if (_musicVolumeSlider != null)
            _musicVolumeSlider.value = MusicVolume;

        if (_sfxVolumeSlider != null)
            _sfxVolumeSlider.value = SFXVolume;
    }
    #endregion

    //Audio Sources
    [SerializeField] AudioSource _musicAudioSource;
    [SerializeField] AudioSource _sfxAudioSource;

    //Sliders
    [SerializeField] Slider _masterVolumeSlider;
    [SerializeField] Slider _sfxVolumeSlider;
    [SerializeField] Slider _musicVolumeSlider;

    // Volume Vars
    public float MasterVolume = 1;
    public float MusicVolume = 1;
    public float SFXVolume = 1;

    #region Sliders

    public Slider MasterVolumeSlider
    {
        get => _masterVolumeSlider;
        set
        {
            if (PlayerPrefs.HasKey("MasterVolume"))
            {
                MasterVolume = PlayerPrefs.GetFloat("MasterVolume");
                _musicAudioSource.volume = MasterVolume * MusicVolume;
                _sfxAudioSource.volume = MasterVolume * SFXVolume;
            }
            _masterVolumeSlider = value;
            _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            _masterVolumeSlider.value = MasterVolume;
        }
    }

    public Slider MusicVolumeSlider
    {
        get => _musicVolumeSlider;
        set
        {
            MusicVolume = _musicAudioSource.volume;
            if (PlayerPrefs.HasKey("MusicVolume"))
            {
                MusicVolume = PlayerPrefs.GetFloat("MusicVolume");
                _musicAudioSource.volume = MasterVolume * MusicVolume;
            }
            _musicVolumeSlider = value;
            _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            _musicVolumeSlider.value = MusicVolume;
        }
    }

    public Slider SFXVolumeSlider
    {
        get => _sfxVolumeSlider;
        set
        {
            SFXVolume = _sfxAudioSource.volume;
            if (PlayerPrefs.HasKey("SFXVolume"))
            {
                SFXVolume = PlayerPrefs.GetFloat("SFXVolume");
                _sfxAudioSource.volume = MasterVolume * SFXVolume;
            }
            _sfxVolumeSlider = value;
            _sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            _sfxVolumeSlider.value = MusicVolume;
        }
    }

    #endregion


    public void PlaySong(AudioClip _clip)
    {
        _musicAudioSource.clip = _clip;
        _musicAudioSource.Play();
    }

    public void PlaySFX(AudioClip _clip)
    {
        _sfxAudioSource.clip = _clip;
        float volume = MasterVolume * MusicVolume;
        _sfxAudioSource.PlayOneShot(_clip, volume);

    }

    public void OnMasterVolumeChanged(float _volume)
    {
        MasterVolume = _volume;
        _musicAudioSource.volume = MasterVolume * MusicVolume;
        _sfxAudioSource.volume = MasterVolume * SFXVolume;
        PlayerPrefs.SetFloat("MasterVolume", _volume);
    }

    public void OnMusicVolumeChanged(float _volume)
    {
        MusicVolume = _volume;
        _musicAudioSource.volume = MasterVolume * MusicVolume;
        PlayerPrefs.SetFloat("MusicVolume", _volume);
    }

    public void OnSFXVolumeChanged(float _volume)
    {
        SFXVolume = _volume;
        _sfxAudioSource.volume = SFXVolume;
        PlayerPrefs.SetFloat("SFXVolume", _volume);
    }
}
