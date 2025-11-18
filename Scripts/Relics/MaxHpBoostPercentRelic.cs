using UnityEngine;

public class MaxHpBoostPercentRelic : BaseRelic
{
    private float percentageBonus = 0.25f; // 5%
    private static int maxHpPercentRelicCount = 0;

    protected override int CurrentRelicCount
    {
        get => maxHpPercentRelicCount;
        set => maxHpPercentRelicCount = value;
    }

    protected override string TooltipDescription =>
        $"+{percentageBonus * 100}% Max HP";

    protected override void ApplyEffect()
    {
        int increase = Mathf.RoundToInt(PlayerController.maxHp * percentageBonus);
        PlayerController.maxHp += increase;
        PlayerController.currentHp += increase; // opzionale, così cura anche la differenza
    }
}
