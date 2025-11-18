using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider masterSlider;
    public AudioMixer myMixer;
    public AudioSource musicSource;

    public GameObject mainMenuPanel;
    public GameObject optionsPanel;

    void Start()
    {
        if (PlayerPrefs.HasKey("MasterVolume") || PlayerPrefs.HasKey("MusicVolume") || PlayerPrefs.HasKey("SfxVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
            SetSfxVolume();
            SetMasterVolume();
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (PauseMenuManager.resetMainMenu)
        {
            Destroy(gameObject);
            PauseMenuManager.resetMainMenu = false;
        }

        if (PauseMenuManager.isPause)
        {
            musicSource.Pause();
        }
        else if (!PauseMenuManager.isPause)
        {
            musicSource.UnPause();
        }
    }

    public void SetMasterVolume()
    {
        float volume = masterSlider.value;
        myMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        myMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSfxVolume()
    {
        float volume = sfxSlider.value;
        myMixer.SetFloat("SfxVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SfxVolume", volume);
    }

    private void LoadVolume()
    {
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SfxVolume", 1f);
        SetMusicVolume();
        SetSfxVolume();
        SetMasterVolume();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
        //DontDestroyOnLoad(gameObject);
        mainMenuPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}
