using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static int attackBonus = 0;
    public static int dexterity = 1;
    public static int critChance = 5;
    public static int critDamage = 200;

    public static int maxHp = 10;
    public static int currentHp;
    public static int shieldBonus = 0;
    public static int playerCurrentShield;
    public static int maxShield;
    public static int healBonus = 0;
    public static int hpBoostOnLevelUp = 10;

    public static int currentXp;
    public static int xpPerEnemyDefeated = 50;
    public static int xpTresholdLevel = 100;
    public static int currentLevel = 1; // LIVELLO ATTUALE

    public static int currentCash = 100;

    static public int attackLevel = 1;
    static public int shieldLevel = 1;
    static public int healLevel = 1;

    public static List<int> attackFlatBonuses = new List<int>();
    public static List<float> attackMultBonuses = new List<float>();

    public static List<int> shieldFlatBonuses = new List<int>();
    public static List<float> shieldMultBonuses = new List<float>();

    public static List<int> healFlatBonuses = new List<int>();
    public static List<float> healMultBonuses = new List<float>();

    public static bool isCrit;
    public static bool attacking = false;
    public static bool shielding = false;
    public static bool healing = false;

    public static int attackTotal;
    public static int healTotal;
    public static int shieldTotal;

    public static int finalDamage;
    public static int finalShield;
    public static int finalHeal;

    public GameObject dicesPanel;
    public GameObject levelUpPanel;
    public GameObject playerCanvas;
    public GameObject enemyCanvas;
    public GameObject rollButton;
    public GameObject shopPanel;

    public TMP_Text attackLevelText;
    public TMP_Text shieldLevelText;
    public TMP_Text healLevelText;

    public Slider playerHpSlider;
    public Image fillImageHp;
    public TMP_Text playerHpText;

    public Slider xpSlider; // SLIDER XP
    public TMP_Text xpText; // TESTO XP tipo "30 / 150 XP"
    public TMP_Text levelText; // TESTO LIVELLO ATTUALE
    public Image fillImageXp;

    public TMP_Text floorText;
    static public float floorNumber = 1f;

    public TMP_Text currentCashText;
    static public int cashForKill = 3;

    private Color darkGreen = new Color(0.2f, 0.5f, 0f);
    private Color xpBarColor = new Color(0.1f, 0.4f, 0.4f);

    static public bool playerTurn = true;
    static public bool playerTurnEnded = false;
    static public bool enemyTurn = false;

    public TMP_Text turnsPassedText;
    static public int turnsPassed = 0;
    static public int enemiesDefeated = 0;
    static public bool noHitsTaken = true;

    static public bool isUpgrading = false;

    //RELICS
    public static int currentFreeRerolls = 0;
    public static int maxFreeRerolls;

    public static int attPerCashBonus = 0;

    public static int lastAppliedTurn_AttEvery10 = 0;

    public AudioSource audioSource;
    public AudioClip levelUpSound;

    private void Start()
    {
        UpdateUI();
        currentHp = maxHp;
    }

    void Update()
    {
        if (playerTurn)
        {
            dicesPanel.SetActive(true);
        }

        else if (!playerTurn && enemyTurn)
        {
            dicesPanel.SetActive(false);
            playerTurnEnded = true;
        }

        if (AttBoostEvery10TurnsRelic.relicCount > 0)
        {
            int interval = 10;
            int currentStep = turnsPassed / interval;

            int lastStep = lastAppliedTurn_AttEvery10 / interval;

            if (currentStep > lastStep)
            {
                int attGain = 24 * AttBoostEvery10TurnsRelic.relicCount;
                PlayerController.attackFlatBonuses.Add(attGain);
                PlayerController.attackBonus += attGain;

                lastAppliedTurn_AttEvery10 = turnsPassed;

                // Debug o effetto visivo
                //Debug.Log("+ " + attGain + " ATT gained from AttEvery12TurnsRelic!");
            }
        }

        // LEVEL UP CHECK
        if (currentXp >= xpTresholdLevel && floorNumber != 24)
        {
            LevelUp();
        }

        UpdateUI();

        UpdateAttPerCashBonus();
    }

    void UpdateUI()
    {
        // HP BAR
        if (playerHpSlider != null)
        {
            playerHpSlider.maxValue = maxHp;
            playerHpSlider.value = Mathf.Clamp(currentHp, 0, maxHp);
            fillImageHp.color = darkGreen;
        }

        if (playerHpText != null)
        {
            playerHpText.text = $"{Mathf.Ceil(currentHp)} / {maxHp}";
        }

        if (playerHpText != null)
        {
            currentCashText.text = $"{currentCash} $";
        }

        // XP BAR + TEXT
        if (xpSlider != null)
        {
            xpSlider.maxValue = xpTresholdLevel;
            xpSlider.value = currentXp;
            //fillImageXp.color = xpBarColor;
        }

        if (xpText != null)
        {
            xpText.text = $"{currentXp} / {xpTresholdLevel} XP";
        }

        // LEVEL TEXT
        if (levelText != null)
        {
            levelText.text = $"Level: {currentLevel}";
        }

        if (attackLevelText != null)
        {
            attackLevelText.text = "LVL: " + attackLevel;
        }

        if (shieldLevelText != null)
        {
            shieldLevelText.text = "LVL: " + shieldLevel;
        }

        if (healLevelText != null)
        {
            healLevelText.text = "LVL: " + healLevel;
        }

        floorText.text = "Floor: " + Mathf.RoundToInt(floorNumber) + "/" + 24;
    }

    void LevelUp()
    {
        if (floorNumber != 24)
        {
            isUpgrading = true;
            audioSource.PlayOneShot(levelUpSound);
            levelUpPanel.SetActive(true);
            currentLevel++; // AUMENTA LIVELLO
            currentXp = currentXp - xpTresholdLevel; // RESETTA XP
            xpTresholdLevel = Mathf.RoundToInt(xpTresholdLevel * 1.5f); // +50% XP RICHIESTI
            maxHp += hpBoostOnLevelUp;
            currentHp = Mathf.Min(currentHp + hpBoostOnLevelUp, maxHp);
        }
    }

    public static int CalculateFinalDamage()
    {
        int flatSum = 0;
        foreach (int b in attackFlatBonuses)
            flatSum += b;

        float multProduct = 1f;
        foreach (float m in attackMultBonuses)
            multProduct *= m;

        int damageWithFlat = attackTotal + flatSum;
        float damageWithMult = damageWithFlat * multProduct;

        return Mathf.RoundToInt(damageWithMult);
    }

    public void IncreaseAttack()
    {
        attackBonus += 10;
        attackLevel += 1;
        attackFlatBonuses.Add(10);
        levelUpPanel.SetActive(false);
        isUpgrading = false;
    }

    public void IncreaseShield()
    {
        shieldBonus += 10;
        shieldLevel += 1;
        shieldFlatBonuses.Add(10);
        levelUpPanel.SetActive(false);
        isUpgrading = false;
    }

    public void IncreaseHeal()
    {
        healBonus += 10;
        healLevel += 1;
        healFlatBonuses.Add(10);
        levelUpPanel.SetActive(false);
        isUpgrading = false;
    }

    public void GetCash()
    {
        currentCash += 10;
        levelUpPanel.SetActive(false);
        isUpgrading = false;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public static void UpdateAttPerCashBonus()
    {
        if (AttBoostPerCashRelic.attBoostPerCashRelicCount <= 0) return;

        int newBonus = currentCash * 1 * AttBoostPerCashRelic.attBoostPerCashRelicCount;

        if (newBonus != attPerCashBonus)
        {
            // Rimuovi vecchio bonus
            attackFlatBonuses.Remove(attPerCashBonus);

            // Aggiungi nuovo bonus
            attackFlatBonuses.Add(newBonus);

            attPerCashBonus = newBonus;
        }
    }



}
