using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public abstract class BaseRelic : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private string relicTooltipText;

    protected GameObject tooltipPanel;
    protected TMP_Text tooltipText;
    protected TMP_Text relicAmountText;
    public string rarity;

    protected abstract int CurrentRelicCount { get; set; }
    protected abstract string TooltipDescription { get; }

    protected virtual void Start()
    {
        tooltipPanel = transform.Find("RelicTooltipPanel")?.gameObject;
        tooltipText = transform.GetComponentInChildren<TMP_Text>(true);

        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);

        /*foreach (TMP_Text tmp in GetComponentsInChildren<TMP_Text>(true))
        {
            if (tmp.name == "RelicAmountNumber")
            {
                relicAmountText = tmp;
                relicAmountText.text = CurrentRelicCount.ToString();
                relicAmountText.gameObject.SetActive(CurrentRelicCount > 1);
                break;
            }
        }*/
    }

    protected virtual void Update()
    {
        /*if (relicAmountText != null)
        {
            relicAmountText.text = CurrentRelicCount.ToString();
            relicAmountText.gameObject.SetActive(CurrentRelicCount > 1);
        } */
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null && tooltipText != null)
        {
            tooltipText.text = TooltipDescription;
            tooltipPanel.SetActive(true);
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (ShopManager.inShop)
        {
            ApplyEffect();
            CurrentRelicCount += 1;
        }
    }

    protected abstract void ApplyEffect();
}
