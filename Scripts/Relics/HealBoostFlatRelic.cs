using UnityEngine;

public class HealBoostFlatRelic : BaseRelic
{
    private int bonus = 5;
    private static int healBoostFlatRelicCount = 0;

    protected override int CurrentRelicCount
    {
        get => healBoostFlatRelicCount;
        set => healBoostFlatRelicCount = value;
    }

    protected override string TooltipDescription => $"+{bonus} Heal Power";

    protected override void ApplyEffect()
    {
        PlayerController.healFlatBonuses.Add(bonus);
    }
}
