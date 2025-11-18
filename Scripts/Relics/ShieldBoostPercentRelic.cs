using UnityEngine;

public class ShieldBoostPercentRelic : BaseRelic
{
    private float bonus = 0.25f; 
    private static int shieldBoostPercentRelicCount = 0;

    protected override int CurrentRelicCount
    {
        get => shieldBoostPercentRelicCount;
        set => shieldBoostPercentRelicCount = value;
    }

    protected override string TooltipDescription => "+ 25% Shield Power";

    protected override void ApplyEffect()
    {
        PlayerController.shieldMultBonuses.Add(1 + bonus);
    }
}
