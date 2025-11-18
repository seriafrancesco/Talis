using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider masterSlider;
    public AudioMixer myMixer;
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    static public bool isPause = false;
    static public bool resetMainMenu;

    void Start()
    {
        LoadVolume();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && PlayerController.currentHp > 0)
        {
            if (!isPause)
                EnterPause();
            else
                ExitPause(); // opzionale, se vuoi togliere la pausa con ESC di nuovo
        }
        if (PlayerController.currentHp <= 0)
        {
            PlayerController.currentHp = 0;
            gameOverPanel.SetActive(true);
            Time.timeScale = 0;
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

    public void EnterPause()
    {
        isPause = true;
        Time.timeScale = 0;
        pausePanel.SetActive(true);
    }

    public void ExitPause()
    {
        isPause = false;
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
        resetMainMenu = true;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }
}
