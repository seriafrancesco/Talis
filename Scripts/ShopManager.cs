using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ShopManager;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class Chest
    {
        public Image chestImage;
        public Button chestButton;
        public string chestSize;
        public ParticleSystem pixelParticles;
        public Transform relicChestSpawnPoint;
        public bool chestOpening;
        public TMP_Text priceTag;
        public GameObject priceTagImage;
    }

    public List<Chest> chests;

    [Header("Chest Sprites")]
    public Sprite smallChestClosed, mediumChestClosed, largeChestClosed;

    [Header("Relic Prefabs Organized By Rarity")]
    public GameObject[] commonRelics;
    public GameObject[] uncommonRelics;
    public GameObject[] rareRelics;
    public GameObject[] mythicRelics;
    public Transform relicParentCanvas;

    [Header("Spawn Setup")]
    public Transform relicGetPoint;
    private int maxPerRow = 37;
    private float xOffset = 50f;
    private float yOffset = 50f;
    private int relicCount = 0;
    public static bool isDuplicate = false;

    [Header("5 Slot Preview System")]
    public Transform[] relicPreviewSlots;
    public Button rerollButton;
    public TMP_Text rerollCostText;
    private int currentRerollCost = 1;
    private GameObject[] currentPreviewRelics = new GameObject[5];
    public Button leaveShopButton;
    public Image coverRerollButton;

    [Header("UI Panels")]
    public GameObject shopPanel;
    public GameObject enemyPanel;
    public GameObject playerPanel;
    public GameObject victoryPanel;

    [Header("Dice Panel Move")]
    public RectTransform dicesPanel;
    private float moveAmountY = 205f;
    private float moveDuration = 1f;
    private bool shopOpened;
    private Coroutine currentMove;
    private bool panelDown = false;

    private bool canLeaveShop;
    private bool chestOpening;
    private bool victoryPanelOn;
    public TMP_Text turnsPassedText;

    [Header("Price Tags")]
    public TMP_Text[] relicPriceTags = new TMP_Text[5]; // 5 preview relics
    public TMP_Text[] chestPriceTags = new TMP_Text[2]; // 2 chest UI
    public GameObject[] relicPriceTagImages = new GameObject[5];

    static public bool inShop;

    void Start()
    {
        SetupAllChests();
        rerollButton.onClick.AddListener(RerollPreviewRelics);
        PreviewRelics();
    }

    void Update()
    {
        Debug.Log(inShop);
        if (EnemyController.enemyDead && !shopOpened && PlayerController.currentXp > 0 && PlayerController.floorNumber != 24 && !PlayerController.isUpgrading)
        {
            Invoke("OpenShop", 2f);
            shopOpened = true;
        }
        else if (EnemyController.enemyDead && PlayerController.floorNumber == 24 && !victoryPanelOn)
        {
            victoryPanel.gameObject.SetActive(true);
            victoryPanelOn = true;
        }

        coverRerollButton.gameObject.SetActive(PlayerController.currentCash < currentRerollCost);

        if (canLeaveShop)
        {
            leaveShopButton.gameObject.SetActive(true);
        }
        else
        {
            leaveShopButton.gameObject.SetActive(false);
        }

        if (PlayerController.currentFreeRerolls > 0)
        {
            rerollCostText.text = "REROLL 0$";
            coverRerollButton.gameObject.SetActive(false);
        }
        else
        {
            rerollCostText.text = "REROLL " + currentRerollCost + "$";
        }
        //Debug.Log(PlayerController.currentFreeRerolls);

        turnsPassedText.text = "Turn: " + PlayerController.turnsPassed;
    }

    void SetupAllChests()
    {
        foreach (var chest in chests)
        {
            SetupSingleChest(chest);
        }

        UpdateChestPriceTags();
    }

    void SetupSingleChest(Chest chest)
    {
        // RESETTO STATO PRIMA DI RIUSARE LA CASSA
        chest.priceTagImage.SetActive(true);
        chest.priceTag.gameObject.SetActive(true);

        chest.chestOpening = false;
        chest.chestImage.color = Color.white;
        chest.chestImage.gameObject.SetActive(true);
        if (chest.pixelParticles != null)
            chest.pixelParticles.Stop();

        // ROLL NUOVA TAGLIA E SPRITE
        int roll = Random.Range(1, 7);
        if (roll <= 4)
        {
            chest.chestSize = "small";
            chest.chestImage.sprite = smallChestClosed;
        }
        else if (roll <= 6)
        {
            chest.chestSize = "medium";
            chest.chestImage.sprite = mediumChestClosed;
        }
        else
        {
            chest.chestSize = "large";
            chest.chestImage.sprite = largeChestClosed;
        }

        // IMPOSTA IL PREZZO CORRETTO
        int price = chest.chestSize == "small" ? 2 :
                    chest.chestSize == "medium" ? 4 : 6;

        if (chest.priceTag != null)
        {
            chest.priceTag.text = price + "$";
            chest.priceTag.gameObject.SetActive(true);
        }

        // RIMUOVO EVENTUALI CLICK PRECEDENTI E AGGIUNGO IL NUOVO
        chest.chestButton.onClick.RemoveAllListeners();
        chest.chestButton.onClick.AddListener(() =>
        {
            if (!chest.chestOpening && PlayerController.currentCash >= price && !chestOpening)
            {
                PlayerController.currentCash -= price;
                if (chest.priceTag != null)
                    chest.priceTag.gameObject.SetActive(false);

                StartCoroutine(OpenChestRoutine(chest));
            }
        });
    }

    IEnumerator OpenChestRoutine(Chest chest)
    {
        chest.priceTagImage.SetActive(false);
        chestOpening = true;
        canLeaveShop = false;
        chest.chestOpening = true;
        RelicResult relicResult = GetRandomRelic(chest.chestSize);
        GameObject relicPrefab = relicResult.relic;
        string rarity = relicResult.rarity;

        if (chest.pixelParticles != null)
        {
            var main = chest.pixelParticles.main;
            Color rarityColor = GetRarityColor(rarity);
            main.startColor = new ParticleSystem.MinMaxGradient(rarityColor, Color.white);
            chest.pixelParticles.Play();
            var renderer = chest.pixelParticles.GetComponent<ParticleSystemRenderer>();
            renderer.sortingLayerName = "UI";
            renderer.sortingOrder = 100; // Più alto di qualunque background
        }

        yield return new WaitForSeconds(3.5f);
        yield return StartCoroutine(FadeOutChestImage(chest));
        yield return new WaitForSeconds(1f);

        chestOpening = false;

        GameObject relicGO = null;
        if (relicPrefab != null)
        {
            relicGO = SpawnRelic(relicPrefab, chest.relicChestSpawnPoint);
        }

        if (relicGO != null)
        {
            Button relicButton = relicGO.GetComponent<Button>() ?? relicGO.AddComponent<Button>();
            GameObject relicFinal = relicPrefab;
            relicButton.onClick.AddListener(() =>
            {
                GetRelic(relicFinal);
                Destroy(relicGO);
            });
        }
    }

    GameObject SpawnRelic(GameObject relicPrefab, Transform spawnPoint)
    {
        Vector3 spawnPos = spawnPoint.localPosition;
        GameObject relicInstance = Instantiate(relicPrefab, relicParentCanvas);
        RectTransform relicRect = relicInstance.GetComponent<RectTransform>();
        relicRect.localPosition = spawnPos;
        relicRect.localScale = Vector3.one * 100f;
        StartCoroutine(ScaleDownRoutine(relicRect));
        return relicInstance;
    }

    void GetRelic(GameObject relicPrefab)
    {
        int row = relicCount / maxPerRow;
        int col = relicCount % maxPerRow;
        Vector3 spawnPos = relicGetPoint.localPosition + new Vector3(col * xOffset, -row * yOffset, 0);

        GameObject relicInstance = Instantiate(relicPrefab, relicParentCanvas);
        RectTransform relicRect = relicInstance.GetComponent<RectTransform>();
        relicRect.localPosition = spawnPos;
        relicRect.localScale = Vector3.one * 50f;
        relicCount++;
        canLeaveShop = true;
    }

    IEnumerator ScaleDownRoutine(RectTransform relicRect)
    {
        Vector3 startScale = Vector3.one * 200f;
        Vector3 endScale = Vector3.one * 100f;
        float duration = 0.05f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            relicRect.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        relicRect.localScale = endScale;
    }

    void PreviewRelics()
    {
        currentRerollCost = 3;
        for (int i = 0; i < currentPreviewRelics.Length; i++)
        {
            if (currentPreviewRelics[i] != null)
                Destroy(currentPreviewRelics[i]);
        }

        for (int i = 0; i < relicPreviewSlots.Length; i++)
        {
            RelicResult relicResult = GetRandomRelic("shop");
            GameObject relicPrefab = relicResult.relic;
            string rarity = relicResult.rarity;
            GameObject relicGO = Instantiate(relicPrefab, relicPreviewSlots[i]);

            if (relicPriceTagImages[i] != null)
                relicPriceTagImages[i].SetActive(true);

            if (relicPriceTags[i] != null)
                relicPriceTags[i].gameObject.SetActive(true);

            BaseRelic baseRelic = relicGO.GetComponent<BaseRelic>();
            if (baseRelic != null)
                baseRelic.rarity = rarity;

            RectTransform rect = relicGO.GetComponent<RectTransform>();
            rect.localPosition = Vector3.zero;
            rect.localScale = Vector3.one * 100f;

            currentPreviewRelics[i] = relicGO;

            Button relicButton = relicGO.GetComponent<Button>() ?? relicGO.AddComponent<Button>();
            GameObject relicFinal = relicPrefab;
            GameObject relicToDestroy = relicGO;

            relicButton.onClick.AddListener(() =>
            {
                int price = GetPriceFromRarity(baseRelic.rarity);
                if (PlayerController.currentCash >= price && inShop)
                {
                    PlayerController.currentCash -= price;
                    GetRelic(relicFinal);
                    Destroy(relicToDestroy);

                    int index = System.Array.IndexOf(currentPreviewRelics, relicToDestroy);
                    if (index >= 0 && relicPriceTagImages[index] != null)
                        relicPriceTagImages[index].SetActive(false);

                    if (index >= 0 && relicPriceTags[index] != null)
                        relicPriceTags[index].gameObject.SetActive(false);
                }
            });
        }

        //currentRerollCost += 2;
        //rerollCostText.text = "REROLL " + currentRerollCost + "$";
        UpdateRelicPriceTags();
    }

    void RerollPreviewRelics()
    {
        bool isFree = PlayerController.currentFreeRerolls > 0;

        if (!isFree && PlayerController.currentCash < currentRerollCost)
            return;

        for (int i = 0; i < currentPreviewRelics.Length; i++)
        {
            if (currentPreviewRelics[i] != null)
                Destroy(currentPreviewRelics[i]);
        }

        for (int i = 0; i < relicPreviewSlots.Length; i++)
        {
            RelicResult relicResult = GetRandomRelic("shop");
            GameObject relicPrefab = relicResult.relic;
            string rarity = relicResult.rarity;
            GameObject relicGO = Instantiate(relicPrefab, relicPreviewSlots[i]);

            if (relicPriceTagImages[i] != null)
                relicPriceTagImages[i].SetActive(true);

            if (relicPriceTags[i] != null)
                relicPriceTags[i].gameObject.SetActive(true);

            BaseRelic baseRelic = relicGO.GetComponent<BaseRelic>();
            if (baseRelic != null)
                baseRelic.rarity = rarity;

            RectTransform rect = relicGO.GetComponent<RectTransform>();
            rect.localPosition = Vector3.zero;
            rect.localScale = Vector3.one * 100f;

            currentPreviewRelics[i] = relicGO;

            Button relicButton = relicGO.GetComponent<Button>() ?? relicGO.AddComponent<Button>();
            GameObject relicFinal = relicPrefab;
            GameObject relicToDestroy = relicGO;

            relicButton.onClick.AddListener(() =>
            {
                int price = GetPriceFromRarity(baseRelic.rarity);
                if (PlayerController.currentCash >= price && inShop)
                {
                    PlayerController.currentCash -= price;
                    GetRelic(relicFinal);
                    Destroy(relicToDestroy);

                    int index = System.Array.IndexOf(currentPreviewRelics, relicToDestroy);
                    if (index >= 0 && relicPriceTagImages[index] != null)
                        relicPriceTagImages[index].SetActive(false);

                    if (index >= 0 && relicPriceTags[index] != null)
                        relicPriceTags[index].gameObject.SetActive(false);
                }
            });
        }

        if (isFree)
        {
            PlayerController.currentFreeRerolls -= 1;
        }
        else
        {
            PlayerController.currentCash -= currentRerollCost;
            currentRerollCost += 2;
        }
        rerollCostText.text = isFree ? "REROLL 0$" : "REROLL " + currentRerollCost + "$";
        UpdateRelicPriceTags();
    }

    void UpdateRelicPriceTags()
    {
        for (int i = 0; i < currentPreviewRelics.Length; i++)
        {
            if (currentPreviewRelics[i] != null)
            {
                BaseRelic baseRelic = currentPreviewRelics[i].GetComponent<BaseRelic>();
                if (baseRelic != null)
                {
                    int price = GetPriceFromRarity(baseRelic.rarity);
                    relicPriceTags[i].text = price + "$";
                }
            }
        }
    }

    void UpdateChestPriceTags()
    {
        for (int i = 0; i < chests.Count && i < chestPriceTags.Length; i++)
        {
            int price = chests[i].chestSize == "small" ? 2 :
                        chests[i].chestSize == "medium" ? 4 : 6;

            chestPriceTags[i].text = price + "$";
        }
    }

    int GetPriceFromRarity(string rarity)
    {
        switch (rarity.ToLower())
        {
            case "common": return 3;
            case "uncommon": return 5;
            case "rare": return 7;
            case "mythic": return 10;
            default: return 0;
        }
    }

    public struct RelicResult
    {
        public GameObject relic;
        public string rarity;

        public RelicResult(GameObject relic, string rarity)
        {
            this.relic = relic;
            this.rarity = rarity;
        }
    }

    RelicResult GetRandomRelic(string chestSize)
    {
        float rarityRoll = Random.value;

        if (chestSize == "shop")
        {
            if (rarityRoll < 0.5f) return new RelicResult(GetRandomFromArray(commonRelics), "common"); // 50% di possibilità
            if (rarityRoll < 0.8f) return new RelicResult(GetRandomFromArray(uncommonRelics), "uncommon"); // 30% di possibilità (da 0.5 a 0.8)
            return new RelicResult(GetRandomFromArray(rareRelics), "rare"); // 20% di possibilità (da 0.8 a 1)
        }

        if (chestSize == "small")
        {
            if (rarityRoll < 0.6f) return new RelicResult(GetRandomFromArray(commonRelics), "common"); // 60% di possibilità
            if (rarityRoll < 0.9f) return new RelicResult(GetRandomFromArray(uncommonRelics), "uncommon"); // 30% di possibilità (da 0.6 a 0.9)
            return new RelicResult(GetRandomFromArray(rareRelics), "rare"); // 10% di possibilità (da 0.9 a 1)
        }
        else if (chestSize == "medium")
        {
            if (rarityRoll < 0.4f) return new RelicResult(GetRandomFromArray(commonRelics), "common"); // 30% di possibilità
            if (rarityRoll < 0.8f) return new RelicResult(GetRandomFromArray(uncommonRelics), "uncommon"); // 50% di possibilità (da 0.3 a 0.8)
            if (rarityRoll < 0.98f) return new RelicResult(GetRandomFromArray(rareRelics), "rare"); // 18% di possibilità (da 0.8 a 0.98)
            return new RelicResult(GetRandomFromArray(mythicRelics), "mythic"); // 2% di possibilità (da 0.98 a 1)
        }
        else
        {
            if (rarityRoll < 0.1f) return new RelicResult(GetRandomFromArray(commonRelics), "common"); // 10% di possibilità 
            if (rarityRoll < 0.6f) return new RelicResult(GetRandomFromArray(uncommonRelics), "uncommon"); // 50% di possibilità (da 0.1 a 0.6)
            if (rarityRoll < 0.96f) return new RelicResult(GetRandomFromArray(rareRelics), "rare"); // 37% di possibilità (da 0.6 a 0.97)
            return new RelicResult(GetRandomFromArray(mythicRelics), "mythic"); // 4% di possibilità (da 0.96 a 1)
        }
    }

    GameObject GetRandomFromArray(GameObject[] array)
    {
        if (array.Length == 0) return null;
        return array[Random.Range(0, array.Length)];
    }

    Color GetRarityColor(string rarity)
    {
        switch (rarity.ToLower())
        {
            case "common": return Color.green;
            case "uncommon": return Color.blue;
            case "rare": return Color.yellow;
            case "mythic": return new Color(0.6f, 0f, 0.6f);
            default: return Color.white;
        }
    }

    IEnumerator FadeOutChestImage(Chest chest)
    {
        Color c = chest.chestImage.color;
        c.a = 1;
        chest.chestImage.color = c;

        while (c.a > 0)
        {
            c.a -= Time.deltaTime * 5;
            chest.chestImage.color = c;
            yield return null;
        }
    }

    public void CloseShop()
    {
        leaveShopButton.gameObject.SetActive(false);
        shopPanel.gameObject.SetActive(false);
        enemyPanel.gameObject.SetActive(true);
        playerPanel.gameObject.SetActive(true);
        shopOpened = false;
        PlayerController.playerTurn = true;
        MoveUpDicePanel();
        RefreshShop();
        canLeaveShop = false;
        inShop = false;

        panelDown = false;
        turnsPassedText.enabled = true;
    }

    public void OpenShop()
    {
        shopPanel.gameObject.SetActive(true);
        enemyPanel.gameObject.SetActive(false);
        playerPanel.gameObject.SetActive(false);
        MoveDownDicePanel();
        Invoke("EnableLeaveShopButton", moveDuration);
        PlayerController.currentFreeRerolls = PlayerController.maxFreeRerolls;
        inShop = true;

        turnsPassedText.enabled = false;
    }

    public void MoveDownDicePanel()
    {
        if (!panelDown)
        {
            StartSmoothMove(-moveAmountY);
            panelDown = true;
        }
    }

    public void MoveUpDicePanel()
    {
        StartSmoothMove(moveAmountY);
        panelDown = false;
    }

    private void StartSmoothMove(float deltaY)
    {
        if (currentMove != null)
            StopCoroutine(currentMove);

        currentMove = StartCoroutine(SmoothMove(deltaY));
    }

    private IEnumerator SmoothMove(float deltaY)
    {
        Vector2 startPos = dicesPanel.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, deltaY);
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            dicesPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        dicesPanel.anchoredPosition = endPos;

        // BUG FIX EXTRA: aggiorna panelDown correttamente
        panelDown = deltaY < 0;
    }

    void RefreshShop()
    {
        PreviewRelics();
        SetupAllChests();
    }

    void EnableLeaveShopButton()
    {
        canLeaveShop = true;
    }

    public void EndlessMode()
    {
        victoryPanel.gameObject.SetActive(false);
        OpenShop();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
