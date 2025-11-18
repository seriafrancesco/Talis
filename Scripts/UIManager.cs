using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelManager : MonoBehaviour
{
    public GameObject[] panels;
    public Button[] openButtons;
    public Button[] closeButtons;

    public TMP_Text attackLevelText;
    public TMP_Text shieldLevelText;
    public TMP_Text healLevelText;

    public DiceRoller diceRoller;

    void Start()
    {
        // Se vuoi partire con i pannelli chiusi
        // foreach (GameObject panel in panels)
        // {
        //     panel.SetActive(false);
        // }

        for (int i = 0; i < openButtons.Length; i++)
        {
            int index = i;
            openButtons[i].onClick.AddListener(() => OpenPanel(index));
        }
    }

    void Update()
    {
        UpdateLevelTexts();

        if (PlayerController.enemyTurn)
        {
            CloseAllPanels();
        }

        if (PlayerController.playerTurn)
        {
            OpenPanel(0);
        }
    }

    void UpdateLevelTexts()
    {
        if (attackLevelText != null)
        {
            attackLevelText.text = "LVL: " + PlayerController.attackLevel;
        }

        if (shieldLevelText != null)
        {
            shieldLevelText.text = "LVL: " + PlayerController.shieldLevel;
        }

        if (healLevelText != null)
        {
            healLevelText.text = "LVL: " + PlayerController.healLevel;
        }
    }

    public void OpenPanel(int index)
    {
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }

        if (index >= 0 && index < panels.Length)
        {
            panels[index].SetActive(true);
        }
    }

    void ClosePanel(int index)
    {
        if (index >= 0 && index < panels.Length)
        {
            panels[index].SetActive(false);
        }
    }

    void CloseAllPanels()
    {
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
    }
}
