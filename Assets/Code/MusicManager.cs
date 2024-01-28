using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MusicManager : MonoBehaviour
{
    #region SINGLETON
    private static MusicManager _instance = null;
    public static MusicManager Instance { get { return _instance; } }
    private MusicManager() { }
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _audioSource.Play();
        }
    }
    #endregion

    [SerializeField] AudioSource _audioSource;
    [SerializeField] Slider _masterVolumeSlider;

    public Slider MasterVolumeSlider {
        get =>_masterVolumeSlider;
        set
        {
            Volume = _audioSource.volume;
            if (PlayerPrefs.HasKey("MasterVolume"))
            {
                Volume = PlayerPrefs.GetFloat("MasterVolume");
                _audioSource.volume = Volume;
            }
            _masterVolumeSlider = value;
            _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            _masterVolumeSlider.value = Volume;
        }
    }

public float Volume;
    public void PlaySong(AudioClip _clip)
    {
        _audioSource.clip = _clip;
        _audioSource.Play();
    }
    // Start is called before the first frame update
    void Start()
    {
        Volume = _audioSource.volume;

        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            Volume = PlayerPrefs.GetFloat("MasterVolume");
            _audioSource.volume = Volume;
        }
            
        if(_masterVolumeSlider != null)
            _masterVolumeSlider.value = Volume;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMasterVolumeChanged(float  _volume)
    {
        Volume = _volume;
        _audioSource.volume = Volume;
        PlayerPrefs.SetFloat("MasterVolume", _volume);
    }
}
