using UnityEngine;

public class XPBoostPercentageRelic : BaseRelic
{
    private float bonusMultiplier = 0.2f;
    private static int xpBoostPercentageRelic = 0;

    protected override int CurrentRelicCount
    {
        get => xpBoostPercentageRelic;
        set => xpBoostPercentageRelic = value;
    }

    protected override string TooltipDescription => "+20% XP when killing an enemy.";

    protected override void ApplyEffect()
    {
        PlayerController.xpPerEnemyDefeated = Mathf.RoundToInt(PlayerController.xpPerEnemyDefeated * (1 + bonusMultiplier));
    }
}
