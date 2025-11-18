using UnityEngine;

public class HealBoostPercentRelic : BaseRelic
{
    private float bonus = 0.25f;
    private static int healBoostPercentRelicCount = 0;

    protected override int CurrentRelicCount
    {
        get => healBoostPercentRelicCount;
        set => healBoostPercentRelicCount = value;
    }

    protected override string TooltipDescription => "+25% Heal Power";

    protected override void ApplyEffect()
    {
        PlayerController.healMultBonuses.Add(1f + bonus);
    }
}
