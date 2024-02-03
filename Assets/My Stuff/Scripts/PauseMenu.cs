using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public bool IsPaused { get; private set; } = false;

    public AudioMixer SFXAudioMixer;
    public AudioMixer MusicAudioMixer;

    public Toggle sfxToggle;
    public Toggle musicToggle;

    const float minVolume = -80f;

    private float sfxVolume;
    private float musicVolume;


    private void Awake()
    {
        sfxVolume = PlayerPrefs.GetFloat("sfx", 0);
        musicVolume = PlayerPrefs.GetFloat("music", 0);

        sfxToggle.isOn = sfxVolume == 0;
        musicToggle.isOn = musicVolume == 0;
    }

    public void ChangePause()
    {
        IsPaused = !IsPaused;
        Cursor.lockState = IsPaused ? CursorLockMode.None : CursorLockMode.Locked;
        gameObject.SetActive(IsPaused);
    }

    public void ChangeFullscreen(Toggle toggle)
    {

    }
    public void ChangeMusic(Toggle toggle)
    {
        musicVolume = toggle.isOn ? 0 : minVolume;
        MusicAudioMixer.SetFloat("volume", musicVolume);
        PlayerPrefs.SetFloat("music", musicVolume);
        PlayerPrefs.Save();
    }
    public void ChangeSFX(Toggle toggle)
    {
        sfxVolume = toggle.isOn ? 0 : minVolume;
        SFXAudioMixer.SetFloat("volume", sfxVolume);
        PlayerPrefs.SetFloat("sfx", sfxVolume);
        PlayerPrefs.Save();
    }
}
