using UnityEngine;

public class MaxHpBoostFlatRelic : BaseRelic
{
    private int bonus = 10;
    private static int maxHpRelicCount = 0;

    protected override int CurrentRelicCount
    {
        get => maxHpRelicCount;
        set => maxHpRelicCount = value;
    }

    protected override string TooltipDescription => "+10 Max HP";

    protected override void ApplyEffect()
    {
        PlayerController.maxHp += bonus;
        PlayerController.currentHp = Mathf.Min(PlayerController.currentHp + bonus, PlayerController.maxHp);
    }
}
