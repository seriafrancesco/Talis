using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DiceRoller : MonoBehaviour
{
    public Image[] diceImages;
    public Sprite[] attackDiceFaces;
    public Sprite[] shieldDiceFaces;
    public Sprite[] healDiceFaces;
    public Image[] selectionHighlights;
    public Image playerImage;

    public TMP_Text attackTotalText;
    public TMP_Text shieldTotalText;
    public TMP_Text healTotalText;

    public Image shieldIcon;
    public TMP_Text shieldValueText;

    static public int selectableDices = 2;

    public AudioSource audioSource;
    //public AudioClip rollStartSound;
    public AudioClip numberSound;
    public AudioClip faceRevealedSound;
    private float currentPitch = 1f;
    private float pitchIncrement = 0.1f;
    private float timeDopamine = 1f;

    private DiceType[] diceTypes = new DiceType[6];
    private bool[] isSelected = new bool[6];
    private int[] diceValues = new int[6];

    private bool isRolling = false;

    public enum DiceType { Attack, Shield, Heal }

    void Start()
    {
        diceTypes[0] = DiceType.Attack;
        diceTypes[1] = DiceType.Attack;
        diceTypes[2] = DiceType.Shield;
        diceTypes[3] = DiceType.Shield;
        diceTypes[4] = DiceType.Heal;
        diceTypes[5] = DiceType.Heal;

        for (int i = 0; i < selectionHighlights.Length; i++)
        {
            selectionHighlights[i].enabled = false;
            isSelected[i] = false;
        }

        HideAllTexts();
        ResetDiceFaces();

        shieldIcon.gameObject.SetActive(false);
        shieldValueText.gameObject.SetActive(false);

        audioSource.pitch = currentPitch;
    }

    private void Update()
    {
        if (EnemyController.enemyTurnEnded)
        {
            PlayerController.playerCurrentShield = 0;
            shieldIcon.gameObject.SetActive(false);
            shieldValueText.gameObject.SetActive(false);
            EnemyController.enemyTurnEnded = false;
        }

        if (EnemyController.enemyCurrentHp <= 0 && !EnemyController.enemyDead)
        {
            PlayerController.playerCurrentShield = 0;
            shieldIcon.gameObject.SetActive(false);
            shieldValueText.gameObject.SetActive(false);
            EnemyController.enemyTurnEnded = false;
        }

        if (Input.GetKeyDown(KeyCode.Space)) // esempio: premendo spazio
        {
            audioSource.pitch = currentPitch;
            audioSource.clip = faceRevealedSound;
            audioSource.Play();
            currentPitch += pitchIncrement;
        }

        //Debug.Log(currentPitch);

        if (currentPitch > 3)
        {
            currentPitch = 3;
        }

        if (timeDopamine < 0.3f)
        {
            timeDopamine = 0.3f;
        }

        shieldValueText.text = PlayerController.playerCurrentShield.ToString();
    }

    public void ToggleSelection(int index)
    {
        if (isRolling) return;

        if (!PlayerController.playerTurn) return;

        int selectedCount = 0;
        for (int i = 0; i < isSelected.Length; i++)
        {
            if (isSelected[i]) selectedCount++;
        }

        if (!isSelected[index] && selectedCount >= selectableDices) return;

        isSelected[index] = !isSelected[index];
        selectionHighlights[index].enabled = isSelected[index];
    }

    public void RollSelectedDice()
    {
        if (isRolling) return;

        StartCoroutine(RollDiceCoroutine());
    }

    private IEnumerator RollDiceCoroutine()
    {
        isRolling = true;
        HideAllTexts();

        int attackTotal = 0;
        int shieldTotal = 0;
        int healTotal = 0;

        float rollDuration = 1f;
        float rollTimer = 0f;

        while (rollTimer < rollDuration)
        {
            for (int i = 0; i < diceImages.Length; i++)
            {
                if (!isSelected[i]) continue;

                Sprite[] faceArray = GetFacesArray(diceTypes[i]);
                int r = Random.Range(1, faceArray.Length);
                diceImages[i].sprite = faceArray[r];
            }

            rollTimer += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        for (int i = 0; i < diceImages.Length; i++)
        {
            if (!isSelected[i]) continue;

            Sprite[] faceArray = GetFacesArray(diceTypes[i]);
            diceImages[i].sprite = faceArray[0];
        }

        yield return new WaitForSeconds(1f); //tempo suspense prima di comparire la prima faccia

        for (int i = 0; i < diceImages.Length; i++)
        {
            if (!isSelected[i]) continue;

            Sprite[] faceArray = GetFacesArray(diceTypes[i]);
            int finalValue = Random.Range(1, faceArray.Length);

            diceImages[i].sprite = faceArray[finalValue];
            diceValues[i] = finalValue;

            switch (diceTypes[i])
            {
                case DiceType.Attack: attackTotal += finalValue; break;
                case DiceType.Shield: shieldTotal += finalValue; break;
                case DiceType.Heal: healTotal += finalValue; break;
            }
            audioSource.pitch = currentPitch;
            audioSource.clip = numberSound;
            audioSource.Play();
            currentPitch += pitchIncrement;


            yield return new WaitForSeconds(0.5f);
        }

        for (int i = 0; i < isSelected.Length; i++)
        {
            isSelected[i] = false;
            selectionHighlights[i].enabled = false;
        }

        currentPitch = 1;

        if (shieldTotal > 0)
        {
            PlayerController.shieldTotal = shieldTotal;
            shieldTotalText.gameObject.SetActive(true);

            yield return StartCoroutine(ShowShieldGainStepByStep(shieldTotal));
        }

        if (healTotal > 0)
        {
            PlayerController.healTotal = healTotal;
            healTotalText.gameObject.SetActive(true);

            yield return StartCoroutine(ShowHealGainStepByStep(healTotal));
        }

        if (attackTotal > 0)
        {
            PlayerController.attackTotal = attackTotal;
            attackTotalText.gameObject.SetActive(true);

            yield return StartCoroutine(ShowAttackDamageStepByStep(attackTotal));
        }

        ResetDiceFaces();

        isRolling = false;
        if (EnemyController.enemyDead == false)
        {
            PlayerController.enemyTurn = true;
            PlayerController.playerTurn = false;
        }
    }

    // Metti questo dentro DiceRoller, sotto il resto:
    private IEnumerator ShowAttackDamageStepByStep(int baseDamage)
    {
        int currentDamage = baseDamage;

        audioSource.pitch = currentPitch;
        audioSource.clip = faceRevealedSound;
        audioSource.Play();
        currentPitch += pitchIncrement;
        attackTotalText.text = currentDamage.ToString();
        yield return new WaitForSeconds(1f);

        // Bonus flat uno a uno
        foreach (int flatBonus in PlayerController.attackFlatBonuses)
        {
            currentDamage += flatBonus;
            attackTotalText.text = currentDamage.ToString();
            audioSource.pitch = currentPitch;
            audioSource.clip = faceRevealedSound;
            audioSource.Play();
            currentPitch += pitchIncrement;
            timeDopamine -= 0.1f;
            yield return new WaitForSeconds(timeDopamine);
        }

        // Bonus percentuali (moltiplicatori) uno a uno
        foreach (float multiplier in PlayerController.attackMultBonuses)
        {
            currentDamage = Mathf.RoundToInt(currentDamage * multiplier);
            attackTotalText.text = currentDamage.ToString();
            audioSource.pitch = currentPitch;
            audioSource.clip = faceRevealedSound;
            audioSource.Play();
            currentPitch += pitchIncrement;
            timeDopamine -= 0.1f;
            yield return new WaitForSeconds(timeDopamine);
        }

        PlayerController.finalDamage = currentDamage; // aggiorna danno finale
        attackTotalText.text = currentDamage.ToString();
        yield return new WaitForSeconds(1f);
        StartCoroutine(ShakeSpriteAttack());
        attackTotalText.gameObject.SetActive(false);
        EnemyController.enemyTakingDamage = true;
        yield return new WaitForSeconds(1f);
        EnemyController.buffCompleted = false;
        PlayerController.turnsPassed += 1;
        PlayerController.playerTurnEnded = true;
        currentPitch = 1;
        timeDopamine = 1;

        if (HpRegenOnAttackRelic.healAmount > 0)
        {
            PlayerController.currentHp = Mathf.Min(PlayerController.currentHp + HpRegenOnAttackRelic.healAmount, PlayerController.maxHp);
        }

        if (HpRegenAfterTurnRelic.healAmount > 0)
        {
            PlayerController.currentHp = Mathf.Min(PlayerController.currentHp + HpRegenAfterTurnRelic.healAmount, PlayerController.maxHp);
        }

    }

    IEnumerator ShakeSpriteAttack()
    {
        Vector3 originalPosition = playerImage.transform.position;

        float elapsedTime = 0f;
        while (elapsedTime < EnemyController.shakeTime)
        {
            float shake = Mathf.Sin(elapsedTime * Mathf.PI * EnemyController.shakeSpeed) * EnemyController.shakeAmount;
            playerImage.transform.position = originalPosition + new Vector3(shake, 0, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerImage.transform.position = originalPosition;
    }

    private IEnumerator ShowShieldGainStepByStep(int baseShield)
    {
        int currentShield = baseShield;

        audioSource.pitch = currentPitch;
        audioSource.clip = faceRevealedSound;
        audioSource.Play();
        currentPitch += pitchIncrement;
        shieldTotalText.text = currentShield.ToString();
        yield return new WaitForSeconds(1f);

        // Aggiunge bonus flat uno a uno
        foreach (int flatBonus in PlayerController.shieldFlatBonuses)
        {
            currentShield += flatBonus;
            shieldTotalText.text = currentShield.ToString();
            audioSource.pitch = currentPitch;
            audioSource.clip = faceRevealedSound;
            audioSource.Play();
            currentPitch += pitchIncrement;
            timeDopamine -= 0.1f;
            yield return new WaitForSeconds(timeDopamine);
        }

        // Aggiunge bonus moltiplicatori uno a uno
        foreach (float multiplier in PlayerController.shieldMultBonuses)
        {
            currentShield = Mathf.RoundToInt(currentShield * multiplier);
            shieldTotalText.text = currentShield.ToString();
            audioSource.pitch = currentPitch;
            audioSource.clip = faceRevealedSound;
            audioSource.Play();
            currentPitch += pitchIncrement;
            timeDopamine -= 0.1f;
            yield return new WaitForSeconds(timeDopamine);
        }

        PlayerController.finalShield = currentShield;
        PlayerController.playerCurrentShield = PlayerController.finalShield;
        shieldTotalText.text = currentShield.ToString();
        yield return new WaitForSeconds(1f);
        shieldValueText.gameObject.SetActive(true);
        shieldValueText.text = PlayerController.finalShield.ToString();
        shieldIcon.gameObject.SetActive(true);
        shieldTotalText.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        EnemyController.buffCompleted = false;
        PlayerController.turnsPassed += 1;
        PlayerController.playerTurnEnded = true;
        currentPitch = 1;
        timeDopamine = 1;

        if (HpRegenAfterTurnRelic.healAmount > 0)
        {
            PlayerController.currentHp = Mathf.Min(PlayerController.currentHp + HpRegenAfterTurnRelic.healAmount, PlayerController.maxHp);
        }

        if (CursedShieldRelic.relicCount > 0 && PlayerController.maxHp > 1)
        {
            PlayerController.maxHp = Mathf.Max(1, PlayerController.maxHp - 1);

            int shieldBonus = CursedShieldRelic.relicCount * CursedShieldRelic.shieldBonusPerRelic;
            PlayerController.shieldFlatBonuses.Add(shieldBonus);

            //Debug.Log("[CursedShieldRelic] -1 MaxHP, +" + shieldBonus + " Shield Power");
        }

        //currentShield += amount;
    }

    private IEnumerator ShowHealGainStepByStep(int baseHeal)
    {
        int currentHeal = baseHeal;

        audioSource.pitch = currentPitch;
        audioSource.clip = faceRevealedSound;
        audioSource.Play();
        currentPitch += pitchIncrement;
        healTotalText.text = currentHeal.ToString();
        yield return new WaitForSeconds(1f);

        // Aggiunge bonus flat uno a uno
        foreach (int flatBonus in PlayerController.healFlatBonuses)
        {
            currentHeal += flatBonus;
            healTotalText.text = currentHeal.ToString();
            audioSource.pitch = currentPitch;
            audioSource.clip = faceRevealedSound;
            audioSource.Play();
            currentPitch += pitchIncrement;
            timeDopamine -= 0.1f;
            yield return new WaitForSeconds(timeDopamine);
        }

        // Aggiunge bonus moltiplicatori uno a uno
        foreach (float multiplier in PlayerController.healMultBonuses)
        {
            currentHeal = Mathf.RoundToInt(currentHeal * multiplier);
            healTotalText.text = currentHeal.ToString();
            audioSource.pitch = currentPitch;
            audioSource.clip = faceRevealedSound;
            audioSource.Play();
            currentPitch += pitchIncrement;
            timeDopamine -= 0.1f;
            yield return new WaitForSeconds(timeDopamine);
        }

        PlayerController.finalHeal = currentHeal;
        healTotalText.text = currentHeal.ToString();
        yield return new WaitForSeconds(1f);
        PlayerController.currentHp = Mathf.Min(PlayerController.currentHp + currentHeal, PlayerController.maxHp);
        healTotalText.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        EnemyController.buffCompleted = false;
        PlayerController.turnsPassed += 1;
        PlayerController.playerTurnEnded = true;
        currentPitch = 1;
        timeDopamine = 1;

        if (HpRegenAfterTurnRelic.healAmount > 0)
        {
            PlayerController.currentHp = Mathf.Min(PlayerController.currentHp + HpRegenAfterTurnRelic.healAmount, PlayerController.maxHp);
        }
    }



    private Sprite[] GetFacesArray(DiceType type)
    {
        switch (type)
        {
            case DiceType.Attack: return attackDiceFaces;
            case DiceType.Shield: return shieldDiceFaces;
            case DiceType.Heal: return healDiceFaces;
        }

        return null;
    }

    private void HideAllTexts()
    {
        attackTotalText.gameObject.SetActive(false);
        shieldTotalText.gameObject.SetActive(false);
        healTotalText.gameObject.SetActive(false);
    }

    private void ResetDiceFaces()
    {
        for (int i = 0; i < diceImages.Length; i++)
        {
            Sprite[] faces = GetFacesArray(diceTypes[i]);
            if (faces != null && faces.Length > 0)
            {
                diceImages[i].sprite = faces[0];
            }
        }

        for (int i = 0; i < selectionHighlights.Length; i++)
        {
            selectionHighlights[i].enabled = false;
            isSelected[i] = false;
        }
    }
}