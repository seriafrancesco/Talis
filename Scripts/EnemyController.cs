using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Audio;

public class EnemyController : MonoBehaviour
{
    public static float enemyCurrentHp;
    private int enemyMaxHp = 10;
    static public bool enemyTakingDamage;

    private float enemyDamage = 3;
    private float enemyHealPower = 3;
    private float enemyShieldPower = 3;
    public static int enemyCurrentShield;

    public Slider enemyHpSlider;
    public Image fillImage;
    public GameObject enemyHpBar;
    public TMP_Text enemyHpText;
    public TMP_Text enemyActionText;

    public Image enemyShieldImage;
    public TMP_Text enemyShieldText;

    //private Color green = Color.green;
    private Color darkRed = new Color(0.6f, 0f, 0f);
    //private Color red = Color.red;

    static public bool enemyTurnEnded;

    public Image enemyImage;
    public Sprite[] possibleEnemySprites;

    static public float shakeAmount = 20f;
    static public float shakeSpeed = 2f;
    static public float shakeTime = 0.5f;

    private bool enemyChoiceMade = false;
    private int nextAction = 0;
    private float buffPercent = 0.2f; // +15% boost
    static public bool buffCompleted;
    private int buffsThisRound;
    static public bool enemyDead = false;

    private int preparedActionValue = 0; // valore già calcolato per l'azione

    public AudioSource audioSource;
    public AudioClip hitSound;

    void Start()
    {
        enemyCurrentHp = enemyMaxHp;
        enemyCurrentShield = 0;
        SetRandomEnemyImage();
        UpdateUI();
        PlayerController.playerTurn = true;
        enemyChoiceMade = false;
    }

    void Update()
    {
        //Debug.Log("enemyturn " + PlayerController.enemyTurn);
        //Debug.Log(HpRegenOnKillRelic.hpRegenOnKillRelicCount);

        UpdateUI();

        if (PlayerController.playerTurn && !enemyChoiceMade)
        {
            ChooseEnemyAction();
        }

        if (PlayerController.enemyTurn && !enemyDead)
        {
            PerformEnemyAction();
        }

        if (enemyCurrentHp <= 0 && !enemyDead)
        {
            enemyCurrentHp = 0;
            Debug.Log("Nemico sconfitto!");
            PlayerController.enemiesDefeated++;
            PlayerController.currentXp += Mathf.RoundToInt(PlayerController.xpPerEnemyDefeated);

            if (PlayerController.noHitsTaken)
            {
                PlayerController.currentCash += PlayerController.cashForKill + 2;
            }
            else
            {
                PlayerController.currentCash += PlayerController.cashForKill;
            }

            //StartCoroutine(HandleDeath());
            PlayerController.enemyTurn = false;
            PlayerController.playerTurn = false;
            PlayerController.turnsPassed = 0;
            PlayerController.lastAppliedTurn_AttEvery10 = 0;

            //BuffStatsByPercent();

            if (HpRegenOnKillRelic.hpRegenOnKillRelicCount > 0)
            {
                float totalPercent = 0.10f * HpRegenOnKillRelic.hpRegenOnKillRelicCount;
                int healAmount = Mathf.CeilToInt(PlayerController.maxHp * totalPercent);
                PlayerController.currentHp = Mathf.Min(PlayerController.currentHp + healAmount, PlayerController.maxHp);
            }

            if (AttBoostPerKillRelic.attBoostPerKillRelicCount > 0)
            {
                AttBoostPerKillRelic.bonus += AttBoostPerKillRelic.attBoostPerKillRelicCount;

                // Aggiorna bonus flat
                PlayerController.attackFlatBonuses.Add(AttBoostPerKillRelic.attBoostPerKillRelicCount);
            }

            if (MaxHpBoostOnKillRelic.relicCount > 0)
            {
                PlayerController.maxHp += MaxHpBoostOnKillRelic.relicCount;
                PlayerController.currentHp = Mathf.Min(PlayerController.currentHp + MaxHpBoostOnKillRelic.relicCount * 2, PlayerController.maxHp);
                Debug.Log("+ " + MaxHpBoostOnKillRelic.relicCount + " Max HP from relic");
            }
            enemyDead = true;
        }

        if (enemyCurrentHp <= 0 && buffsThisRound > 0)
        {
            DebuffStatsByPercent();
        }

        if (enemyTakingDamage)
        {
            TakeDamage();
            enemyTakingDamage = false;
        }

        if (PlayerController.playerTurnEnded)
        {
            enemyCurrentShield = 0;
        }

        if (PlayerController.turnsPassed % 10 == 0 && PlayerController.turnsPassed > 0 && buffCompleted == false)
        {
            BuffStatsByPercent();
            Debug.Log($"Turn {PlayerController.turnsPassed}: Enemy stats buffed!");
            buffsThisRound++;
        }
    }

    public void SetRandomEnemyImage()
    {
        if (possibleEnemySprites.Length == 0 || enemyImage == null) return;

        int randomIndex = Random.Range(0, possibleEnemySprites.Length);
        enemyImage.sprite = possibleEnemySprites[randomIndex];
    }

    public void EnemyRespawn()
    {
        enemyCurrentHp = enemyMaxHp;
        enemyCurrentShield = 0;
        enemyDead = false;
        PlayerController.noHitsTaken = true;
        SetRandomEnemyImage();
        PlayerController.floorNumber += 1;
        PlayerController.turnsPassed = 0;
    }
    public void BuffStatsByPercent()
    {
        float multiplier = 1 + buffPercent;

        enemyMaxHp = Mathf.RoundToInt(enemyMaxHp * multiplier);
        enemyDamage = enemyDamage * multiplier;
        enemyHealPower = enemyHealPower * multiplier;
        enemyShieldPower = enemyShieldPower * multiplier;

        Debug.Log($"Boosted stats: HP {enemyMaxHp}, Damage {enemyDamage}, Heal {enemyHealPower}, Shield {enemyShieldPower}");
        buffCompleted = true;
    }

    public void DebuffStatsByPercent()
    {
        float multiplier = 1 + buffPercent;

        //enemyMaxHp = Mathf.RoundToInt(enemyMaxHp / multiplier);
        enemyDamage = enemyDamage / multiplier;
        enemyHealPower = enemyHealPower / multiplier;
        enemyShieldPower = enemyShieldPower / multiplier;

        buffsThisRound--;
    }

    void UpdateUI()
    {
        if (enemyHpSlider != null)
        {
            enemyHpSlider.maxValue = enemyMaxHp;
            enemyHpSlider.value = Mathf.Clamp(enemyCurrentHp, 0, enemyMaxHp);

            float percent = enemyCurrentHp / enemyMaxHp;

            if (percent > 0.6f)
                fillImage.color = darkRed;
            else if (percent > 0.3f)
                fillImage.color = darkRed;
            else
                fillImage.color = darkRed;
        }

        if (enemyHpText != null)
        {
            enemyHpText.text = $"{Mathf.Ceil(enemyCurrentHp)} / {enemyMaxHp}";
        }

        if (enemyCurrentShield > 0)
        {
            enemyShieldImage.enabled = true;
            enemyShieldText.text = $"{Mathf.Ceil(enemyCurrentShield)}";
        }
        else
        {
            enemyShieldText.text = "";
            enemyShieldImage.enabled = false;
        }
    }

    public void TakeDamage()
    {
        StartCoroutine(ShakeSpriteDefend());
        ApplyDamage();
    }

    IEnumerator ShakeSpriteDefend()
    {
        audioSource.PlayOneShot(hitSound);
        Vector3 originalPosition = enemyImage.transform.position;

        float elapsedTime = 0f;
        while (elapsedTime < shakeTime)
        {
            float shake = Mathf.Sin(elapsedTime * Mathf.PI * shakeSpeed) * shakeAmount;
            enemyImage.transform.position = originalPosition + new Vector3(shake, 0, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        enemyImage.transform.position = originalPosition;
    }

    IEnumerator ShakeSpriteAttack()
    {
        Vector3 originalPosition = enemyImage.transform.position;

        float elapsedTime = 0f;
        while (elapsedTime < shakeTime)
        {
            float shake = Mathf.Sin(elapsedTime * Mathf.PI * shakeSpeed) * shakeAmount;
            enemyImage.transform.position = originalPosition + new Vector3(-shake, 0, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        enemyImage.transform.position = originalPosition;
    }

    public void ChooseEnemyAction()
    {
        nextAction = Random.Range(1, 6); // 1–5
        enemyChoiceMade = true;

        float randomMultiplier = Random.Range(0.5f, 1.5f);

        switch (nextAction)
        {
            case 1:
            case 2:
            case 3:
                preparedActionValue = Mathf.RoundToInt(enemyDamage * randomMultiplier);
                enemyActionText.text = $"Will attack for {preparedActionValue}!";
                break;
            case 4:
                preparedActionValue = Mathf.RoundToInt(enemyShieldPower * randomMultiplier);
                enemyActionText.text = $"Will shield for {preparedActionValue}!";
                break;
            case 5:
                preparedActionValue = Mathf.RoundToInt(enemyHealPower * randomMultiplier);
                enemyActionText.text = $"Will heal for {preparedActionValue}!";
                break;
        }

        Debug.Log($"Azione scelta dal nemico: {nextAction} con valore {preparedActionValue} (moltiplicatore {randomMultiplier:F2})");
    }

    void PerformEnemyAction()
    {
        switch (nextAction)
        {
            case 1:
            case 2:
            case 3:
                StartCoroutine(Attack());
                break;
            case 4:
                StartCoroutine(Shield());
                break;
            case 5:
                StartCoroutine(Heal());
                break;
        }

        nextAction = 0;
        enemyActionText.text = "";
        PlayerController.enemyTurn = false;
        enemyChoiceMade = false;
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(1f);

        Debug.Log($"Nemico ATTACCA per {preparedActionValue}");

        int damageToApply = preparedActionValue;

        if (PlayerController.playerCurrentShield > 0)
        {
            if (PlayerController.playerCurrentShield >= damageToApply)
            {
                PlayerController.playerCurrentShield -= damageToApply;
                damageToApply = 0;
            }
            else
            {
                damageToApply -= PlayerController.playerCurrentShield;
                PlayerController.playerCurrentShield = 0;
            }
        }

        StartCoroutine(ShakeSpriteAttack());
        PlayerController.currentHp -= damageToApply;
        audioSource.PlayOneShot(hitSound);
        if (damageToApply > 0)
        {
            PlayerController.noHitsTaken = false;
        }

        yield return new WaitForSeconds(1f);
        PlayerController.turnsPassed += 1;
        buffCompleted = false;
        enemyTurnEnded = true;
        PlayerController.playerTurn = true;
    }

    private IEnumerator Shield()
    {
        PlayerController.playerTurnEnded = false;
        yield return new WaitForSeconds(1f);

        enemyCurrentShield = preparedActionValue;
        Debug.Log($"Nemico si SHIELDA per {preparedActionValue}");

        yield return new WaitForSeconds(1f);
        PlayerController.turnsPassed += 1;
        buffCompleted = false;
        enemyTurnEnded = true;
        PlayerController.playerTurn = true;
    }

    private IEnumerator Heal()
    {
        yield return new WaitForSeconds(1f);

        enemyCurrentHp = Mathf.Min(enemyCurrentHp + preparedActionValue, enemyMaxHp);
        Debug.Log($"Nemico si CURA di {preparedActionValue}");

        yield return new WaitForSeconds(1f);
        PlayerController.turnsPassed += 1;
        buffCompleted = false;
        enemyTurnEnded = true;
        PlayerController.playerTurn = true;
    }

    private void ApplyDamage()
    {
        Debug.Log("Final Damage: " + PlayerController.finalDamage);
        int incomingDamage = PlayerController.finalDamage;

        if (enemyCurrentShield > 0)
        {
            if (enemyCurrentShield >= incomingDamage)
            {
                enemyCurrentShield -= incomingDamage;
                incomingDamage = 0;
            }
            else
            {
                incomingDamage -= enemyCurrentShield;
                enemyCurrentShield = 0;
            }
        }

        enemyCurrentHp -= incomingDamage;
    }
}